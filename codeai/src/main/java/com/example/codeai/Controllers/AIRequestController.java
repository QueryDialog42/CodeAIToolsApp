package com.example.codeai.Controllers;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

import lombok.RequiredArgsConstructor;
import lombok.experimental.var;

import java.nio.charset.StandardCharsets;
import org.springframework.http.MediaType;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import com.example.codeai.Dtos.AIRequestDto;
import org.springframework.core.io.Resource;
import com.example.codeai.Dtos.AIResponseDto;
import com.example.codeai.Dtos.SettingDto;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.client.RestTemplate;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.example.codeai.Dtos.innerDtos.MessageDto;
import com.example.codeai.Entities.Settings;
import com.example.codeai.Mappers.ISettingMapper;
import com.example.codeai.Repositories.ISettingsRepository;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.mongo.StandardMongoClientSettingsBuilderCustomizer;

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
    private final ISettingMapper settingMapper;
    private final ISettingsRepository settingsRepository;
    private final RestTemplate restTemplate = new RestTemplate();

    @Value("classpath:system-explain-prompt.txt")
    private Resource systemExplainPrompt;

    @Value("classpath:system-transform-prompt-java.txt")
    private Resource systemTransformPromptForJava;

    @Value("classpath:system-transform-prompt-cpp.txt")
    private Resource systemTransformPromptForCpp;

    @Value("classpath:system-transform-prompt-python.txt")
    private Resource systemTransformPromptForPython;

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

    @GetMapping("/getModels/{userId}")
    private ResponseEntity<List<String>> GetAiModels(
            @PathVariable Integer userId) {
        try {

            Settings setting = settingsRepository.findByUserId(userId);
            SettingDto settingDto = settingMapper.ToDto(setting);         

            HttpClient client = HttpClient.newHttpClient();

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(settingDto.getBaseUrl()))
                    .header("Authorization", "Bearer " + settingDto.getApiKey())
                    .header("Content-Type", "application/json")
                    .GET()
                    .build();

            HttpResponse<String> response = client.send(request,
                    HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() == 308) {
                // Handle redirect - get new location
                String location = response.headers().firstValue("Location").orElse(null);
                if (location != null) {
                    // Follow redirect with new location
                    HttpRequest redirectRequest = HttpRequest.newBuilder()
                            .uri(URI.create(location))
                            .header("Authorization", "Bearer " + settingDto.getApiKey())
                            .header("Content-Type", "application/json")
                            .GET()
                            .build();
                    
                    HttpResponse<String> redirectResponse = client.send(redirectRequest,
                            HttpResponse.BodyHandlers.ofString());
                    
                    if (redirectResponse.statusCode() == 200) {
                        // Parse JSON → extract model IDs
                        ObjectMapper mapper = new ObjectMapper();
                        JsonNode root = mapper.readTree(redirectResponse.body());
                        JsonNode data = root.path("data");

                        List<String> models = new ArrayList<>();
                        if (data.isArray()) {
                            for (JsonNode node : data) {
                                String id = node.path("id").asText(null);
                                if (id != null) models.add(id);
                            }
                        }

                        return ResponseEntity.ok(models);
                    }
                }
                return ResponseEntity.status(response.statusCode()).build();
            } else if (response.statusCode() != 200) {
                return ResponseEntity.status(response.statusCode()).build();
            }

            // Parse JSON → extract model IDs
            ObjectMapper mapper = new ObjectMapper();
            JsonNode root = mapper.readTree(response.body());
            JsonNode data = root.path("data");

            // Sort models by created timestamp (newest first)
            List<Map.Entry<String, Long>> modelEntries = new ArrayList<>();
            if (data.isArray()) {
                for (JsonNode node : data) {
                    String id = node.path("id").asText(null);
                    long created = node.path("created").asLong(0);
                    if (id != null) modelEntries.add(Map.entry(id, created));
                }
            }
            modelEntries.sort((a, b) -> Long.compare(b.getValue(), a.getValue()));
            List<String> models = modelEntries.stream()
                .map(Map.Entry::getKey)
                .collect(Collectors.toList());
            return ResponseEntity.ok(models);

        } catch (Exception ex) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @GetMapping("/getBaseKey/{userId}")
    private ResponseEntity<List<String>> GetBaseUrlAndApiKey(
        @PathVariable Integer userId) {
        Settings setting = settingsRepository.findByUserId(userId);
        if (setting == null) {
            System.out.println("Setting not found for user: " + userId);
            return ResponseEntity.notFound().build();
        }
        return ResponseEntity.ok(List.of(setting.getBaseUrl(), setting.getApiKey(), setting.getSendUrl()));
    }

    @PostMapping("/saveBaseKey/{userId}")
    private ResponseEntity<Void> SaveBaseUrlAndApiKey(
        @PathVariable Integer userId,
        @RequestBody List<String> baseUrlAndApiKey
    ){

        Settings setting = new Settings();
        setting.setUserId(userId);
        setting.setBaseUrl(baseUrlAndApiKey.get(0));
        setting.setApiKey(baseUrlAndApiKey.get(1)); 
        setting.setSendUrl(baseUrlAndApiKey.get(2));  

        Settings existingUser = settingsRepository.findByUserId(userId);
        if (existingUser != null) {
            settingsRepository.delete(existingUser);
        }
        settingsRepository.save(setting);
        return ResponseEntity.ok().build();
    }

    private AIRequestDto setAiTransformRequestDto(AIRequestDto aiRequestDto) throws IOException {
        return setDtoBySystemPrompt(aiRequestDto, new String(ChooseLanguageToParse(aiRequestDto), StandardCharsets.UTF_8));
    }

    private AIRequestDto setAiExplainRequestDto(AIRequestDto aiRequestDto) throws IOException {
        return setDtoBySystemPrompt(aiRequestDto, new String(systemExplainPrompt.getInputStream().readAllBytes(), StandardCharsets.UTF_8));
    }

    private AIResponseDto handleAIResponse(AIRequestDto aiRequestDto) throws JsonProcessingException {
        Settings setting = settingsRepository.findByUserId(aiRequestDto.getActiveUserId());

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.set("Authorization", "Bearer " + setting.getApiKey());

        // Sadece OpenRouter'ın beklediği field'lar
        Map<String, Object> body = new java.util.LinkedHashMap<>();
        body.put("model", aiRequestDto.getModel());
        body.put("messages", aiRequestDto.getMessages());
        body.put("temperature", 0.7);
        body.put("max_tokens", 2048);

        String bodyJson = objectMapper.writeValueAsString(body);

        HttpEntity<String> requestEntity = new HttpEntity<>(bodyJson, headers);

        String chatUrl = setting.getSendUrl();

        ResponseEntity<String> response = restTemplate.postForEntity(chatUrl, requestEntity, String.class);
        return objectMapper.readValue(response.getBody(), AIResponseDto.class);
    }

    private AIRequestDto setDtoBySystemPrompt(AIRequestDto aiRequestDto, String systemPrompt) {

        MessageDto systemRole = new MessageDto();
        systemRole.setRole("system");
        systemRole.setContent(systemPrompt); // the system prompt

        var userRole = new MessageDto();
        userRole.setRole("assistant");
        userRole.setContent(aiRequestDto.getMessages().get(aiRequestDto.getMessages().size() - 1).getContent()); // Flow.txt content

        var roleList = new ArrayList<MessageDto>();
        roleList.add(systemRole);
        roleList.add(userRole);

        aiRequestDto.setModel(aiRequestDto.getModel()); // ai model
        aiRequestDto.setMessages(roleList);

        return aiRequestDto;
    }

    private byte[] ChooseLanguageToParse(AIRequestDto aiRequestDto) throws IOException {
        return switch (aiRequestDto.getLanguageToParse()) {
            case "C++" -> systemTransformPromptForCpp.getInputStream().readAllBytes();
            case "Python" -> systemTransformPromptForPython.getInputStream().readAllBytes();
            default -> systemTransformPromptForJava.getInputStream().readAllBytes();
        };
    }
}
