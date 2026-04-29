package com.example.codeai.Dtos;

public class SubscriptionPlanDto {
    private String plan_name;
    private Double plan_price;
    private String plan_duration;  // "monthly", "yearly"
    private String[] features;

    // Constructors
    public SubscriptionPlanDto() {}

    public SubscriptionPlanDto(String plan_name, Double plan_price, String plan_duration, String[] features) {
        this.plan_name = plan_name;
        this.plan_price = plan_price;
        this.plan_duration = plan_duration;
        this.features = features;
    }

    // Getters and Setters
    public String getPlan_name() { return plan_name; }
    public void setPlan_name(String plan_name) { this.plan_name = plan_name; }

    public Double getPlan_price() { return plan_price; }
    public void setPlan_price(Double plan_price) { this.plan_price = plan_price; }

    public String getPlan_duration() { return plan_duration; }
    public void setPlan_duration(String plan_duration) { this.plan_duration = plan_duration; }

    public String[] getFeatures() { return features; }
    public void setFeatures(String[] features) { this.features = features; }
}
