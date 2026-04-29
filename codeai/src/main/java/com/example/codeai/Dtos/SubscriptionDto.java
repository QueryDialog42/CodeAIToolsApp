package com.example.codeai.Dtos;

import java.sql.Timestamp;

public class SubscriptionDto {
    private Integer s_id;
    private Integer u_id;
    private String s_plan;  // "free", "premium", "pro"
    private Timestamp s_start_date;
    private Timestamp s_end_date;
    private Boolean s_is_active;
    private Timestamp s_created_at;
    private Timestamp s_updated_at;

    // Constructors
    public SubscriptionDto() {}

    public SubscriptionDto(Integer s_id, Integer u_id, String s_plan, Timestamp s_start_date, 
                          Timestamp s_end_date, Boolean s_is_active, Timestamp s_created_at, 
                          Timestamp s_updated_at) {
        this.s_id = s_id;
        this.u_id = u_id;
        this.s_plan = s_plan;
        this.s_start_date = s_start_date;
        this.s_end_date = s_end_date;
        this.s_is_active = s_is_active;
        this.s_created_at = s_created_at;
        this.s_updated_at = s_updated_at;
    }

    // Getters and Setters
    public Integer getS_id() { return s_id; }
    public void setS_id(Integer s_id) { this.s_id = s_id; }

    public Integer getU_id() { return u_id; }
    public void setU_id(Integer u_id) { this.u_id = u_id; }

    public String getS_plan() { return s_plan; }
    public void setS_plan(String s_plan) { this.s_plan = s_plan; }

    public Timestamp getS_start_date() { return s_start_date; }
    public void setS_start_date(Timestamp s_start_date) { this.s_start_date = s_start_date; }

    public Timestamp getS_end_date() { return s_end_date; }
    public void setS_end_date(Timestamp s_end_date) { this.s_end_date = s_end_date; }

    public Boolean getS_is_active() { return s_is_active; }
    public void setS_is_active(Boolean s_is_active) { this.s_is_active = s_is_active; }

    public Timestamp getS_created_at() { return s_created_at; }
    public void setS_created_at(Timestamp s_created_at) { this.s_created_at = s_created_at; }

    public Timestamp getS_updated_at() { return s_updated_at; }
    public void setS_updated_at(Timestamp s_updated_at) { this.s_updated_at = s_updated_at; }
}


