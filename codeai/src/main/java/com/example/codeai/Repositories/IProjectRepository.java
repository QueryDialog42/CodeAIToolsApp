package com.example.codeai.Repositories;

import java.util.List;
import java.util.Optional;
import com.example.codeai.Entities.Projects;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.JpaRepository;

public interface IProjectRepository extends JpaRepository<Projects, Integer> {
    @Query(value = "SELECT * FROM projects WHERE belongs_to = :u_id;", nativeQuery = true)
    List<Projects> getAllProjectsById(@Param("u_id") Integer u_id);

    @Query(value = "SELECT * FROM projects WHERE p_name = :p_name;", nativeQuery = true)
    Optional<Projects> findByProjectName(@Param("p_name") String p_name);
}
