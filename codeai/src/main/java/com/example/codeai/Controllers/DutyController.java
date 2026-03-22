package com.example.codeai.Controllers;

import java.util.List;
import lombok.AllArgsConstructor;
import com.example.codeai.Dtos.UserDto;
import com.example.codeai.Dtos.GetDutyDto;
import com.example.codeai.Dtos.ProjectDto;
import com.example.codeai.Entities.Duties;
import com.example.codeai.Mappers.IUserMapper;
import org.springframework.http.ResponseEntity;
import com.example.codeai.Mappers.IProjectMapper;
import org.springframework.web.bind.annotation.*;
import com.example.codeai.Repositories.IDutyRepository;
import com.example.codeai.Repositories.IProjectRepository;
import com.example.codeai.Entities.innerClasses.DutyId;

@RestController
@AllArgsConstructor
@RequestMapping("/duty")
public class DutyController {

    private final IUserMapper userMapper;
    private final IProjectMapper projectMapper;
    private final IDutyRepository dutyRepository;
    private final IProjectRepository projectRepository;

    @PostMapping("/save")
    private ResponseEntity<Void> saveDuty(@RequestBody GetDutyDto getDutyDto) {

        var project = projectRepository.findByProjectName(getDutyDto.getProject_name());
        if (project.isEmpty()) return ResponseEntity.notFound().build();

        var duties = getDutyDto.getWorker_id().stream()
                .map(workerId -> {
                    var dutyId = new DutyId();
                    dutyId.setProject_id(project.get().getP_id());
                    dutyId.setWorker_id(workerId);

                    var duty = new Duties();
                    duty.setDutyId(dutyId);
                    return duty;
                })
                .toList();

        dutyRepository.saveAll(duties);
        return ResponseEntity.ok().build();
    }

    @GetMapping("/get/{p_id}")
    private ResponseEntity<List<UserDto>> sendWorkers(@PathVariable("p_id") Integer project_id) {
        var workers = dutyRepository.findWorkersByProjectId(project_id);
        if (workers.isEmpty()) return ResponseEntity.notFound().build();

        return ResponseEntity.ok(userMapper.toDtos(workers));
    }

    @DeleteMapping("/delete")
    private ResponseEntity<Void> deleteDuty(
            @RequestHeader("p_id") Integer project_id,
            @RequestHeader("w_id") Integer worker_id){
        dutyRepository.deleteDuty(project_id, worker_id);
        return ResponseEntity.ok().build();
    }

    @GetMapping("/getDuties/{worker_id}")
    public ResponseEntity<List<ProjectDto>> getAllProjectById(@PathVariable Integer worker_id) {
        var projectIds = dutyRepository.findProjectIdsByWorkerId(worker_id);
        var projects = projectRepository.findAllById(projectIds);
        var projectDtos = projectMapper.toDtos(projects);
        return ResponseEntity.ok(projectDtos);
    }
}
