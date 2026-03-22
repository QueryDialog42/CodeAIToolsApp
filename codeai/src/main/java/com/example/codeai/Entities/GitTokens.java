package com.example.codeai.Entities;

import lombok.Data;
import jakarta.persistence.*;

@Data
@Entity
@Table(name = "git_tokens")
public class GitTokens {
    @Id
    @Column(name = "belongs_to")
    private Integer belongs_to;

    @OneToOne(fetch = FetchType.LAZY, cascade = CascadeType.ALL)
    @MapsId
    @JoinColumn(name = "belongs_to", referencedColumnName = "u_id")
    private Users user;

    @Column(name = "git_token", nullable = false)
    private String git_token;
}
