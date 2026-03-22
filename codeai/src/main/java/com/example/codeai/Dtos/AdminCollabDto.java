package com.example.codeai.Dtos;

import lombok.Data;

import java.util.List;

@Data
public class AdminCollabDto {
    List<UserDto> Admins;
    List<UserDto> Collaborators;
}
