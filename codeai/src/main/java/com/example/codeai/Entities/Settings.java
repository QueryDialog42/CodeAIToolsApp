package com.example.codeai.Entities;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Data;

@Data
@Entity
@Table(name = "settings")
public class Settings {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "user_id")
    private Integer userId;

    @Column(name = "base_url")
    private String baseUrl;

    @Column(name = "send_url")
    private String sendUrl;

    @Column(name = "api_key")
    private String apiKey;
}
