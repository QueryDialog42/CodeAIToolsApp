package com.example.codeai.Entities;

import lombok.Data;
import jakarta.persistence.*;
import com.example.codeai.Entities.innerClasses.DutyId;

@Data
@Entity
@Table(name = "duties")
public class Duties {

    @EmbeddedId
    private DutyId dutyId;

    @ManyToOne(fetch = FetchType.LAZY, cascade = CascadeType.ALL)
    @JoinColumn(name = "project_id", referencedColumnName = "p_id", updatable = false, insertable = false)
    private Projects project_id;

    @ManyToOne(fetch = FetchType.LAZY, cascade = CascadeType.ALL)
    @JoinColumn(name = "worker_id", referencedColumnName = "u_id", updatable = false, insertable = false)
    private Users worker_id;
}
