package com.example.codeai.Entities;

import java.security.Timestamp;
import java.time.LocalDateTime;

import org.hibernate.annotations.CreationTimestamp;

import jakarta.persistence.CascadeType;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.FetchType;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;
import lombok.Data;

@Data
@Entity
@Table(name = "denies")
public class Denies {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;
    
    @JoinColumn(name = "project_id", referencedColumnName = "p_id")
    private Integer project_id;

    @JoinColumn(name = "admin_id", referencedColumnName = "u_id")
    private Integer admin_id;

    @Column(name = "d_reason")
    private String denieReason;

    private LocalDateTime deniedAt;
}
