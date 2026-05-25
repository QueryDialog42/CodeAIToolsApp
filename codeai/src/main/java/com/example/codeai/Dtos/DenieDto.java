package com.example.codeai.Dtos;

import java.security.Timestamp;
import java.time.LocalDateTime;

import lombok.Data;

@Data
public class DenieDto {
    private Integer project_id;
    private Integer admin_id;
    private String denieReason;
    private LocalDateTime deniedAt;
}
