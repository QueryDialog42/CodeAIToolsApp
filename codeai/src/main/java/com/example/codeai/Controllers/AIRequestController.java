package com.example.codeai.Controllers;

import java.io.IOException;
import java.util.ArrayList;

import com.example.codeai.Dtos.AIResponseDto;
import lombok.RequiredArgsConstructor;
import java.nio.charset.StandardCharsets;
import org.springframework.http.MediaType;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import com.example.codeai.Dtos.AIRequestDto;
import org.springframework.core.io.Resource;
import org.springframework.http.ResponseEntity;
import org.springframework.web.client.RestTemplate;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.example.codeai.Dtos.innerDtos.MessageDto;
import org.springframework.beans.factory.annotation.Value;
import com.fasterxml.jackson.core.JsonProcessingException;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;


@RestController
@RequiredArgsConstructor
@RequestMapping("AI")
public class AIRequestController {

    private final RestTemplate restTemplate = new RestTemplate();
    private final ObjectMapper objectMapper;

    @Value("${ai.model}")
    private String AIModel;

    @Value("${ai.api-key}")
    private String APIKey;

    @Value("${ai.api-url}")
    private String BaseUrl;

    @Value("classpath:system-transform-prompt.txt")
    private Resource systemTransformPrompt;

    @Value("classpath:system-explain-prompt.txt")
    private Resource systemExplainPrompt;

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

    private AIRequestDto setAiTransformRequestDto(AIRequestDto aiRequestDto) throws IOException {
        return setDtoBySystemPrompt(aiRequestDto, new String(systemTransformPrompt.getInputStream().readAllBytes(), StandardCharsets.UTF_8));
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
        userRole.setContent(aiRequestDto.getMessages().getLast().getContent()); // Flow.txt content

        var roleList = new ArrayList<MessageDto>();
        roleList.add(systemRole);
        roleList.add(userRole);

        aiRequestDto.setModel(AIModel);
        aiRequestDto.setMessages(roleList);

        return aiRequestDto;
    }
}
