package com.example.codeai.Dtos;

public class CreateSubscriptionDto {
    private Integer u_id;
    private String s_plan;
    private String payment_method;
    private String payment_token;

    // Constructors
    public CreateSubscriptionDto() {}

    public CreateSubscriptionDto(Integer u_id, String s_plan, String payment_method, String payment_token) {
        this.u_id = u_id;
        this.s_plan = s_plan;
        this.payment_method = payment_method;
        this.payment_token = payment_token;
    }

    // Getters and Setters
    public Integer getU_id() { return u_id; }
    public void setU_id(Integer u_id) { this.u_id = u_id; }

    public String getS_plan() { return s_plan; }
    public void setS_plan(String s_plan) { this.s_plan = s_plan; }

    public String getPayment_method() { return payment_method; }
    public void setPayment_method(String payment_method) { this.payment_method = payment_method; }

    public String getPayment_token() { return payment_token; }
    public void setPayment_token(String payment_token) { this.payment_token = payment_token; }
}
