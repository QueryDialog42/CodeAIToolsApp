package com.example.codeai.Entities;

import lombok.Data;
import jakarta.persistence.*;
import com.example.codeai.Entities.Enums.UserRole;

@Entity
@Table(name = "users")
@Data
public class Users {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer u_id;

    @Column(nullable = false, unique = true)
    private String u_email;

    @Column(nullable = true, unique = true)
    private String u_git_email;

    @Column(nullable = false)
    private String u_email_pass;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private UserRole u_role;
}
