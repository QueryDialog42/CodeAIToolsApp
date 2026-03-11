package com.example.codeai.Dtos;

import lombok.Data;
import lombok.AllArgsConstructor;

@Data
@AllArgsConstructor
public class UserDto {
    private Integer u_id;
    private String u_email;
    private String u_git_email;
    private String u_email_pass;
    private String u_role;
}
