package com.example.codeai.Controllers;

import java.util.List;
import lombok.AllArgsConstructor;
import com.example.codeai.Entities.Users;
import com.example.codeai.Dtos.ProjectDto;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import com.example.codeai.Mappers.IProjectMapper;
import com.example.codeai.Repositories.IUserRepository;
import com.example.codeai.Repositories.IProjectRepository;

@RestController
@AllArgsConstructor
@RequestMapping("/project")
public class ProjectController {

    private final IProjectMapper projectMapper;
    private final IUserRepository userRepository;
    private final IProjectRepository projectRepository;



    @PostMapping("/create")
    private ResponseEntity<ProjectDto> createProject(@RequestBody ProjectDto projectDto) {
        var projectEntity = projectMapper.toEntity(projectDto);

        // set belongs_to id
        projectEntity.setBelongs_to(getOwner(projectDto));

        projectRepository.save(projectEntity);
        return ResponseEntity.ok(projectDto);
    }

    @DeleteMapping("/delete/{p_name}")
    private ResponseEntity<Void> deleteProject(@PathVariable String p_name) {
        var project = projectRepository.findByProjectName(p_name);
        if (project.isPresent()) {
            projectRepository.delete(project.get());
            return ResponseEntity.ok().build();
        }
        return ResponseEntity.notFound().build();
    }

    @GetMapping("/getAll/{u_id}")
    public ResponseEntity<List<ProjectDto>> getAllProjects(@PathVariable Integer u_id) {
        var projects = projectRepository.getAllProjectsById(u_id);
        var projectsDto = projectMapper.toDtos(projects);
        return  ResponseEntity.ok(projectsDto);
    }

    private Users getOwner(ProjectDto projectDto) {
        return userRepository.findById(projectDto.getBelongs_to()).orElseThrow();
    }
}
