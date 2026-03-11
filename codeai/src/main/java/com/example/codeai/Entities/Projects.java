package com.example.codeai.Entities;

import lombok.Data;
import java.sql.Timestamp;
import jakarta.persistence.*;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UpdateTimestamp;


@Entity
@Table(name = "projects")
@Data
public class Projects {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer p_id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "belongs_to", referencedColumnName = "u_id")
    private Users belongs_to;

    @Column(name = "p_name")
    private String p_name;

    @Column(name = "p_size")
    private Integer p_size;

    @Column(name = "p_description")
    private String p_description;

    @CreationTimestamp
    private Timestamp p_create_time;

    @UpdateTimestamp
    private Timestamp p_update_time;
}
