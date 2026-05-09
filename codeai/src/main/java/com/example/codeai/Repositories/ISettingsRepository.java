package com.example.codeai.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.codeai.Entities.Settings;

public interface ISettingsRepository extends JpaRepository<Settings, Integer> {
    Settings findByUserId(Long userId);
}
