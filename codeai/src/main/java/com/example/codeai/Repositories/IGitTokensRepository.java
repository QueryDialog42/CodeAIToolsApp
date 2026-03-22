package com.example.codeai.Repositories;

import java.util.Optional;
import jakarta.transaction.Transactional;
import com.example.codeai.Entities.GitTokens;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.JpaRepository;

public interface IGitTokensRepository extends JpaRepository<GitTokens, Integer> {
    @Query(value = "SELECT git_token FROM git_tokens WHERE git_token = :gitToken", nativeQuery = true)
    Optional<String> findToken(@Param("gitToken") String git_token);

    @Modifying
    @Transactional
    @Query(value = "UPDATE git_tokens SET git_token = :gitToken WHERE belongs_to = :belongsTo", nativeQuery = true)
    int updateToken(@Param("gitToken") String git_token, @Param("belongsTo") Integer belongs_to);
}
