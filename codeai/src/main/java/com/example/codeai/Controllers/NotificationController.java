package com.example.codeai.Controllers;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.codeai.Dtos.NotificationDto;
import com.example.codeai.Repositories.NotificationRepository;

import lombok.AllArgsConstructor;

@RestController
@RequestMapping("/notification")
@AllArgsConstructor
public class NotificationController {

    private final NotificationRepository notificationRepository;

    @GetMapping("/get/{admin_id}")
    private ResponseEntity<List<NotificationDto>> getNotificationsRelated(@PathVariable("admin_id") Integer admin_id){
        // Temp: raw query test
        var raw = notificationRepository.findNotificationsByAdminId(admin_id);
        
        if (!raw.isEmpty()) return ResponseEntity.ok(raw);
        return ResponseEntity.notFound().build();
    }
}
