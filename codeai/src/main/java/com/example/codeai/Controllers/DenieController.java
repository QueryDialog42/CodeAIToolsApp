package com.example.codeai.Controllers;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.codeai.Dtos.DenieDto;
import com.example.codeai.Entities.Denies;
import com.example.codeai.Mappers.IDenieMapper;
import com.example.codeai.Repositories.DenieRepository;

import lombok.AllArgsConstructor;

@RestController
@RequestMapping("/denie")
@AllArgsConstructor
public class DenieController {
    private final DenieRepository denieRepository;
    private final IDenieMapper denieMapper;

    @PostMapping("/create")
    private ResponseEntity<Void> createDenie(
        @RequestBody DenieDto denieDto
    ) {
        var denies = denieMapper.toEntity(denieDto);
        denieRepository.save(denies);
        return ResponseEntity.ok().build();
    }
}
