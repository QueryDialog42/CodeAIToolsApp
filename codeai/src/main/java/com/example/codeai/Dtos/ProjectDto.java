package com.example.codeai.Dtos;

import lombok.Data;
import java.sql.Timestamp;

@Data
public class ProjectDto {
    private Integer belongs_to;
    private String p_name;
    private String p_description;
    private Double p_size;
    private Timestamp p_create_time;
    private Timestamp p_update_time;
}
