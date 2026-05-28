package com.example.codeai.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import com.example.codeai.Entities.Denies;

import jakarta.transaction.Transactional;

public interface DenieRepository extends JpaRepository<Denies, Integer> {
    @Query(value = "SELECT * FROM denied_table WHERE project_id = :projectId", nativeQuery = true)
    Denies findByProjectId(Integer projectId);

    @Modifying
    @Transactional
    @Query(value = "UPDATE denied_table SET d_reason = :newMessage WHERE project_id = :projectId", nativeQuery = true)
    int updateMessageByProjectId(Integer projectId, String newMessage);
}
