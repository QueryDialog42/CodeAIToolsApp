package com.example.codeai.Dtos;

import lombok.Data;

import java.util.List;

@Data
public class DutyDto {
    private Integer project_id;
    private List<Integer> worker_id;
}
