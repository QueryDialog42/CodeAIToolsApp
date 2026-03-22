package com.example.codeai.Entities;


import lombok.Data;
import java.util.List;
import java.util.ArrayList;
import jakarta.persistence.*;
import com.example.codeai.Entities.Enums.UserRole;

@Data
@Entity
@Table(name = "users")
public class Users {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer u_id;

    @Column(nullable = false, unique = true)
    private String u_name;

    @Column(nullable = false, unique = true)
    private String u_email;

    @Column(nullable = true, unique = true)
    private String u_git_email;

    @Column(nullable = false)
    private String u_email_pass;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private UserRole u_role;

    // when user deleted, git token will be also deleted
    @OneToOne(mappedBy = "user", cascade = CascadeType.ALL, orphanRemoval = true)
    private GitTokens gitTokens;

    @OneToMany(mappedBy = "belongs_to", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<Projects> projects = new ArrayList<>();
}
