package com.example.codeai.Controllers;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.codeai.Dtos.NotificationDto;
import com.example.codeai.Repositories.DenieRepository;
import com.example.codeai.Repositories.NotificationRepository;

import lombok.AllArgsConstructor;

@RestController
@RequestMapping("/notification")
@AllArgsConstructor
public class NotificationController {

    private final NotificationRepository notificationRepository;

    @GetMapping("/get/{admin_id}")
    private ResponseEntity<List<NotificationDto>> getNotificationsRelated(Integer admin_id){
        var notifications = notificationRepository.findNotificationsByAdminId(admin_id);
        if (notifications != null){
            return ResponseEntity.ok(notifications);
        }
        return ResponseEntity.notFound().build();
    }
}
