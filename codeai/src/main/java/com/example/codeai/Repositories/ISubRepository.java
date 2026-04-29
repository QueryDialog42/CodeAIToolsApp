package com.example.codeai.Repositories;

import com.example.codeai.Entities.Subs;
import com.example.codeai.Entities.Users;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.Optional;

public interface ISubRepository extends JpaRepository<Subs, Integer> {
    Optional<Subs> findByUser(Users user);
    
    @Query("SELECT s FROM Subs s WHERE s.user.u_id = :userId")
    Optional<Subs> findByUser_U_id(@Param("userId") Integer userId);
    
    @Query("DELETE FROM Subs s WHERE s.user.u_id = :userId")
    void deleteByUser_U_id(@Param("userId") Integer userId);
}
