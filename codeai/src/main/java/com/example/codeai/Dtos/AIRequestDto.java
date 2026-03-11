package com.example.codeai.Dtos;

import lombok.Data;
import java.util.ArrayList;
import com.example.codeai.Dtos.innerDtos.MessageDto;
import com.fasterxml.jackson.annotation.JsonProperty;


@Data
public class AIRequestDto {
    private String model;

    private ArrayList<MessageDto> messages = new ArrayList<>();

    private double temperature = 0.3;

    @JsonProperty("max_tokens")
    private double maxTokens = 0.0;

    @JsonProperty("top_p")
    private double topP = 0.0;

    @JsonProperty("frequency_penalty")
    private double frequencyPenalty = 0.0;

    @JsonProperty("presence_penalty")
    private double presencePenalty = 0.0;
}
