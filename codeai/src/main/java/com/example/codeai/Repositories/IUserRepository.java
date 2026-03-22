package com.example.codeai.Repositories;

import java.util.List;
import java.util.Optional;
import com.example.codeai.Dtos.UserDto;
import com.example.codeai.Entities.Users;
import com.example.codeai.Projections.UserProjection;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.JpaRepository;

public interface IUserRepository extends JpaRepository<Users, Integer> {
    @Query(value = "SELECT u_id, u_name, u_email, u_git_email, u_email_pass, u_role FROM users WHERE u_email = :email", nativeQuery = true)
    Optional<UserDto> findByEmail(@Param("email") String u_email);

    @Query(value = "SELECT u_id, u_name, u_email, u_git_email, u_email_pass, u_role \n" +
            "FROM users \n" +
            "WHERE u_id != :u_id\n" +
            "AND u_id NOT IN (\n" +
            "    SELECT worker_id FROM teams WHERE admin_id = :u_id\n" +
            ");", nativeQuery = true)
    List<UserProjection> findAllUsers(@Param("u_id") Integer u_id);

    @Query(value = "SELECT admin_id FROM teams WHERE worker_id = :workerId;", nativeQuery = true)
    List<Integer> getAllAdminsByWorkerId(@Param("workerId") Integer workerId);
}
