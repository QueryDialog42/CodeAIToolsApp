package com.example.codeai.Repositories;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import com.example.codeai.Entities.Settings;

public interface ISettingsRepository extends JpaRepository<Settings, Integer> {
    @Query(nativeQuery = true, value = "SELECT * FROM settings WHERE user_id = :userId;")
    Settings findByUserId(@Param("userId") Integer userId);
}
