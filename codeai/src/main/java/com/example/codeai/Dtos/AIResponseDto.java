package com.example.codeai.Dtos;

import lombok.Data;
import java.util.ArrayList;
import com.example.codeai.Dtos.innerDtos.MessageDto;

@Data
public class AIResponseDto {
    private String model;
    private ArrayList<ChoiceDto> choices;
    private long created;

    @Data
    public static class ChoiceDto {
        private MessageDto message;
    }
}
