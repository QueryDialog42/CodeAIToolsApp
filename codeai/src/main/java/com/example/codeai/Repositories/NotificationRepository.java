package com.example.codeai.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

import com.example.codeai.Dtos.NotificationDto;
import com.example.codeai.Entities.Denies;

public interface NotificationRepository extends JpaRepository<Denies, Integer> {
    @Query(value = """
    SELECT t.worker_id AS worker_id,
           p.p_name    AS p_name
    FROM denied_table d, teams t, projects p
    WHERE d.admin_id = t.admin_id
      AND t.admin_id = p.belongs_to
      AND d.admin_id = :adminId
      AND p.p_id IN (SELECT project_id FROM denied_table)
    """, nativeQuery = true)
    List<NotificationDto> findNotificationsByAdminId(Integer adminId);
}
