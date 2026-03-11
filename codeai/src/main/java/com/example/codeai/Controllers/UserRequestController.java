package com.example.codeai.Controllers;

import java.util.List;
import java.util.Optional;
import lombok.AllArgsConstructor;
import com.example.codeai.Dtos.*;
import com.example.codeai.Entities.Users;
import com.example.codeai.Mappers.IUserMapper;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import com.example.codeai.Repositories.IUserRepository;



@RestController
@AllArgsConstructor
public class UserRequestController {

    private final IUserMapper userMapper;
    private final IUserRepository userRepository;

    @PostMapping("/register")
    private ResponseEntity<UserDto> userRegisterRequest(@RequestBody UserDto userRegisterDto) {
        Users user = userMapper.ToEntity(userRegisterDto);
        userRepository.save(user);
        return ResponseEntity.ok(userRegisterDto);
    }

    @PostMapping("/login")
    private ResponseEntity<Void> userLoginRequest(@RequestBody UserDto userLoginDto) {
        var u_email = userLoginDto.getU_email();
        var u_email_pass = userLoginDto.getU_email_pass();

        Optional<UserDto> user = userRepository.findByEmail(u_email);
        if (user.isPresent()) {
            if (user.get().getU_email_pass().equals(u_email_pass)) {
                return ResponseEntity.ok().build();
            }
            return ResponseEntity.badRequest().build();
        }
        return ResponseEntity.notFound().build();
    }

    @PostMapping("/getUser")
    private ResponseEntity<Optional<UserDto>> sendActiveUser(@RequestBody UserDto userDto) {
        var loggedUser = userRepository.findByEmail(userDto.getU_email());
        return ResponseEntity.ok(loggedUser);
    }

    @GetMapping("/getAll/{u_id}")
    private ResponseEntity<List<UserDto>> sendAllUsers(@PathVariable Integer u_id){
        var users = userRepository.findAllUsers(u_id);
        var userDtos = userMapper.toDtos(users);

        return ResponseEntity.ok(userDtos);
    }
}
