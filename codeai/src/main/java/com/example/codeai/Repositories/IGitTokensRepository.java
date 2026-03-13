package com.example.codeai.Repositories;

import com.example.codeai.Entities.GitTokens;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.Optional;

public interface IGitTokensRepository extends JpaRepository<GitTokens, Integer> {
    @Query(value = "SELECT git_token FROM git_tokens WHERE git_token = :gitToken", nativeQuery = true)
    Optional<String> findToken(@Param("gitToken") String git_token);
}
