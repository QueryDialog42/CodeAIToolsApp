package com.example.codeai.Entities.innerClasses;

import lombok.Data;
import java.io.Serializable;
import jakarta.persistence.Embeddable;

@Data
@Embeddable
public class TeamsId implements Serializable {
    private Integer admin_id;
    private Integer worker_id;
}
