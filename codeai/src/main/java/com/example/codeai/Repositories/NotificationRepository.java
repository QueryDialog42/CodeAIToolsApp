package com.example.codeai.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import com.example.codeai.Dtos.NotificationDto;
import com.example.codeai.Entities.Denies;

public interface NotificationRepository extends JpaRepository<Denies, Integer> {
    @Query(value = """
    SELECT t.worker_id AS worker_id,
           p.p_name    AS p_name
    FROM denied_table d
    JOIN projects p ON d.project_id = p.p_id
    JOIN teams t    ON t.admin_id   = d.admin_id
    WHERE d.admin_id = :adminId
    LIMIT 200;
    """, nativeQuery = true)
    List<NotificationDto> findNotificationsByAdminId(@Param("adminId") Integer adminId);
}
