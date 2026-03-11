package com.example.codeai.Entities;

import lombok.Data;
import jakarta.persistence.*;
import com.example.codeai.Entities.innerClasses.TeamsId;

@Entity
@Table(name = "teams")
@Data
public class Teams {

    @EmbeddedId
    private TeamsId id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "admin_id", referencedColumnName = "u_id", insertable = false, updatable = false)
    private Users admin;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "worker_id", referencedColumnName = "u_id", insertable = false, updatable = false)
    private Users worker;
}
