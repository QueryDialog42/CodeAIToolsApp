package com.example.codeai.Entities.innerClasses;

import lombok.Data;
import java.io.Serializable;
import jakarta.persistence.Embeddable;

@Data
@Embeddable
public class DutyId implements Serializable {
    private Integer project_id;
    private Integer worker_id;
}
