package com.example.codeai.Controllers;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.codeai.Dtos.DenieDto;
import com.example.codeai.Entities.Denies;
import com.example.codeai.Mappers.IDenieMapper;
import com.example.codeai.Repositories.DenieRepository;
import com.example.codeai.Repositories.IProjectRepository;

import jakarta.websocket.server.PathParam;
import lombok.AllArgsConstructor;

@RestController
@RequestMapping("/denie")
@AllArgsConstructor
public class DenieController {
    private final DenieRepository denieRepository;
    private final IDenieMapper denieMapper;
    private final IProjectRepository projectRepository;

    @PostMapping("/create")
    private ResponseEntity<Void> createDenie(
        @RequestBody DenieDto denieDto
    ) {
        var deniedProject = denieRepository.findByProjectId(denieDto.getProject_id());
        var denies = denieMapper.toEntity(denieDto);
        if (deniedProject == null) {
            denieRepository.save(denies);
            return ResponseEntity.ok().build();
        }
        else{
            denieRepository.updateMessageByProjectId(denieDto.getProject_id(), denieDto.getDenieReason());
            return ResponseEntity.ok().build();
        }
    }

    @GetMapping("/get/{projectName}")
    private ResponseEntity<DenieDto> getDenie(
        @PathVariable String projectName
    ){
        var project = projectRepository.findByProjectName(projectName);
        if (!project.isEmpty()){
            var denie = denieRepository.findByProjectId(project.get().getP_id());
            return ResponseEntity.ok(denieMapper.toDto(denie));
        }
        return ResponseEntity.notFound().build();
    }
}
