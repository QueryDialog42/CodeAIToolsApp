package com.example.codeai.Entities;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Data
@Table(name = "git_tokens")
public class GitTokens {
    @Id
    @Column(name = "belongs_to")
    private Integer belongs_to;

    @OneToOne(fetch = FetchType.LAZY)
    @MapsId
    @JoinColumn(name = "belongs_to", referencedColumnName = "u_id")
    private Users user;

    @Column(name = "git_token", nullable = false)
    private String git_token;
}
