package com.example.codeai.Entities;

import lombok.Data;
import java.sql.Timestamp;
import jakarta.persistence.*;
import org.hibernate.annotations.UpdateTimestamp;
import org.hibernate.annotations.CreationTimestamp;

@Data
@Entity
@Table(name = "subscriptions")
public class Subs {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer s_id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "u_id", referencedColumnName = "u_id", nullable = false)
    private Users user;

    @Column(name = "s_plan", nullable = false)
    private String s_plan;  // "free", "premium", "pro"

    @Column(name = "s_start_date")
    private Timestamp s_start_date;

    @Column(name = "s_end_date")
    private Timestamp s_end_date;

    @Column(name = "s_is_active", nullable = false)
    private Boolean s_is_active = false;

    @CreationTimestamp
    @Column(name = "s_created_at", nullable = false, updatable = false)
    private Timestamp s_created_at;

    @UpdateTimestamp
    @Column(name = "s_updated_at", nullable = false)
    private Timestamp s_updated_at;
}
