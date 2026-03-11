package com.example.codeai.Controllers;

import java.util.List;
import lombok.AllArgsConstructor;
import com.example.codeai.Dtos.UserDto;
import com.example.codeai.Entities.Teams;
import com.example.codeai.Mappers.IUserMapper;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import com.example.codeai.Repositories.ITeamRepository;
import com.example.codeai.Entities.innerClasses.TeamsId;

@RestController
@AllArgsConstructor
@RequestMapping("/teams")
public class TeamController {

    private final IUserMapper userMapper;
    private final ITeamRepository teamRepository;

    @GetMapping("/getCollaborators/{admin_id}")
    private ResponseEntity<List<UserDto>> sendCollaborators(@PathVariable Integer admin_id) {
        var collaborators = teamRepository.getCollaboratorsById(admin_id);
        var collabDtos = userMapper.toDtos(collaborators);
        return ResponseEntity.ok(collabDtos);
    }

    @PostMapping("/addCollaborator/{u_id}")
    private ResponseEntity<Void> addCollaborators(@PathVariable Integer u_id, @RequestBody UserDto userDto) {
        TeamsId teamsId = new TeamsId();
        teamsId.setAdmin_id(u_id);
        teamsId.setWorker_id(userDto.getU_id());

        Teams team = new Teams();
        team.setId(teamsId);

        teamRepository.save(team);
        return ResponseEntity.ok().build();

    }

    @PostMapping("/removeCollaborator/{u_id}")
    private ResponseEntity<Void> removeCollaborator(@PathVariable Integer u_id, @RequestBody UserDto userDto) {
        TeamsId teamsId = new TeamsId();
        teamsId.setAdmin_id(u_id);
        teamsId.setWorker_id(userDto.getU_id());

        Teams team = new Teams();
        team.setId(teamsId);

        teamRepository.delete(team);
        return ResponseEntity.ok().build();
    }
}
