package com.example.codeai.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import com.example.codeai.Entities.Settings;

public interface ISettingsRepository extends JpaRepository<Settings, Integer> {
    @Query(nativeQuery = true, value = "SELECT base_url, api_key FROM settings WHERE user_id = :userId")
    List<String> findByUserId(@Param("userId") String userId);
}
