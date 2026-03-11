package com.example.codeai.Entities.innerClasses;

import lombok.Data;
import java.io.Serializable;
import jakarta.persistence.Embeddable;

@Embeddable
@Data
public class TeamsId implements Serializable {
    private Integer admin_id;
    private Integer worker_id;
}
