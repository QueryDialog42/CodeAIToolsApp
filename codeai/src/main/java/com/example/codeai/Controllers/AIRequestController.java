package com.example.codeai.Controllers;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.ArrayList;
import java.util.List;

import lombok.RequiredArgsConstructor;
import java.nio.charset.StandardCharsets;
import org.springframework.http.MediaType;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import com.example.codeai.Dtos.AIRequestDto;
import org.springframework.core.io.Resource;
import com.example.codeai.Dtos.AIResponseDto;
import org.springframework.http.ResponseEntity;
import org.springframework.web.client.RestTemplate;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.example.codeai.Dtos.innerDtos.MessageDto;
import com.example.codeai.Repositories.ISettingsRepository;

import org.springframework.beans.factory.annotation.Value;
import com.fasterxml.jackson.core.JsonProcessingException;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;


@RestController
@RequiredArgsConstructor
@RequestMapping("AI")
public class AIRequestController {

    private final ObjectMapper objectMapper;
    private final RestTemplate restTemplate = new RestTemplate();

    @Value("${ai.model}")
    private String AIModel;

    @Value("${ai.api-key}")
    private String APIKey;

    @Value("${ai.api-url}")
    private String BaseUrl;

    @Value("classpath:system-explain-prompt.txt")
    private Resource systemExplainPrompt;

    @Value("classpath:system-transform-prompt-java.txt")
    private Resource systemTransformPromptForJava;

    @Value("classpath:system-transform-prompt-cpp.txt")
    private Resource systemTransformPromptForCpp;

    @Value("classpath:system-transform-prompt-python.txt")
    private Resource systemTransformPromptForPython;

    private ISettingsRepository settingsRepository;

    @PostMapping("/transform")
    private ResponseEntity<AIResponseDto> AITransformRequest(@RequestBody AIRequestDto aiRequestDto)
            throws IOException {

        return ResponseEntity.ok(handleAIResponse(setAiTransformRequestDto(aiRequestDto)));
    }

    @PostMapping("/explain")
    private ResponseEntity<AIResponseDto> AIExplainRequest(@RequestBody AIRequestDto aiRequestDto)
            throws IOException {
        return ResponseEntity.ok(handleAIResponse(setAiExplainRequestDto(aiRequestDto)));
    }

    @GetMapping("/getModels")
    private ResponseEntity<List<String>> GetAiModels(
            @RequestParam String apiKey,
            @RequestParam String baseUrl) {
        try {
            HttpClient client = HttpClient.newHttpClient();

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(baseUrl + "/v1/models"))
                    .header("Authorization", "Bearer " + apiKey)
                    .header("Content-Type", "application/json")
                    .GET()
                    .build();

            HttpResponse<String> response = client.send(request,
                    HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                return ResponseEntity.status(response.statusCode()).build();
            }

            // Parse JSON → extract model IDs
            ObjectMapper mapper = new ObjectMapper();
            JsonNode root = mapper.readTree(response.body());
            JsonNode data = root.path("data");

            List<String> models = new ArrayList<>();
            if (data.isArray()) {
                for (JsonNode node : data) {
                    String id = node.path("id").asText(null);
                    if (id != null) models.add(id);
                }
            }

            return ResponseEntity.ok(models);

        } catch (Exception ex) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @GetMapping("/getBaseKey/{userId}")
    private ResponseEntity<List<String>> GetBaseUrlAndApiKey(@RequestParam String userId) {
        var setting = settingsRepository.findByUserId(userId);
        if (setting.isEmpty()) {
            return ResponseEntity.notFound().build();
        }
        return ResponseEntity.ok(List.of(setting.get(0), setting.get(1)));
    }

    private AIRequestDto setAiTransformRequestDto(AIRequestDto aiRequestDto) throws IOException {
        return setDtoBySystemPrompt(aiRequestDto, new String(ChooseLanguageToParse(aiRequestDto), StandardCharsets.UTF_8));
    }

    private AIRequestDto setAiExplainRequestDto(AIRequestDto aiRequestDto) throws IOException {
        return setDtoBySystemPrompt(aiRequestDto, new String(systemExplainPrompt.getInputStream().readAllBytes(), StandardCharsets.UTF_8));
    }

    private AIResponseDto handleAIResponse(AIRequestDto aiRequestDto) throws JsonProcessingException {

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.setBearerAuth(APIKey);

        HttpEntity<AIRequestDto> requestEntity = new HttpEntity<>(aiRequestDto, headers);

        ResponseEntity<String> response = restTemplate.postForEntity(
                BaseUrl,
                requestEntity,
                String.class
        );

        return objectMapper.readValue(response.getBody(), AIResponseDto.class);
    }

    private AIRequestDto setDtoBySystemPrompt(AIRequestDto aiRequestDto, String systemPrompt) {
        var systemRole = new MessageDto();
        systemRole.setRole("system");
        systemRole.setContent(systemPrompt); // the system prompt

        var userRole = new MessageDto();
        userRole.setRole("assistant");
        userRole.setContent(aiRequestDto.getMessages().get(aiRequestDto.getMessages().size() - 1).getContent()); // Flow.txt content

        var roleList = new ArrayList<MessageDto>();
        roleList.add(systemRole);
        roleList.add(userRole);

        aiRequestDto.setModel(AIModel);
        aiRequestDto.setMessages(roleList);

        return aiRequestDto;
    }

    private byte[] ChooseLanguageToParse(AIRequestDto aiRequestDto) throws IOException {
        return switch (aiRequestDto.getLanguageToParse()) {
            case "C++" -> systemTransformPromptForPython.getInputStream().readAllBytes();
            case "Python" -> systemTransformPromptForCpp.getInputStream().readAllBytes();
            default -> systemTransformPromptForJava.getInputStream().readAllBytes();
        };
    }
}
