package com.example.codeai.Repositories;

import java.util.List;
import com.example.codeai.Entities.Teams;
import com.example.codeai.Entities.Users;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ITeamRepository extends JpaRepository<Teams, Integer> {
    @Query(value = "SELECT \n" +
            "    w.u_id,\n" +
            "    w.u_email,\n" +
            "    w.u_git_email,\n" +
            "    w.u_email_pass,\n" +
            "    w.u_role\n" +
            "FROM teams t\n" +
            "JOIN users a ON t.admin_id  = a.u_id\n" +
            "JOIN users w ON t.worker_id = w.u_id\n" +
            "WHERE t.admin_id = :admin_id;", nativeQuery = true)
    List<Users> getCollaboratorsById(@Param("admin_id") Integer admin_id);
}
