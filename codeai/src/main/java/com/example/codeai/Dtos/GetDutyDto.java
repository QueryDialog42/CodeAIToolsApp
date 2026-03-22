package com.example.codeai.Dtos;

import lombok.Data;

import java.util.List;

@Data
public class GetDutyDto {
    private String project_name;
    private List<Integer> worker_id;
}
