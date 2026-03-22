package com.example.codeai.Repositories;

import java.util.List;
import jakarta.transaction.Transactional;
import com.example.codeai.Entities.Duties;
import com.example.codeai.Projections.UserProjection;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.JpaRepository;

public interface IDutyRepository extends JpaRepository<Duties, Integer> {
    @Modifying
    @Transactional
    @Query(value = "DELETE FROM duties WHERE project_id = :projectId", nativeQuery = true)
    void deleteByProjectId(@Param("projectId") Integer projectId);

    @Query(value = "SELECT u.* FROM users u " +
            "INNER JOIN duties d ON u.u_id = d.worker_id " +
            "WHERE d.project_id = :project_id", nativeQuery = true)
    List<UserProjection> findWorkersByProjectId(@Param("project_id") Integer projectId);


    @Query(value = "SELECT project_id FROM duties WHERE worker_id = :workerId", nativeQuery = true)
    List<Integer> findProjectIdsByWorkerId(@Param("workerId") Integer workerId);

    @Modifying
    @Transactional
    @Query(value = "DELETE FROM duties WHERE project_id = :projectId AND worker_id = :workerId", nativeQuery = true)
    void deleteDuty(@Param("projectId") Integer projectId, @Param("workerId") Integer workerId);
}
