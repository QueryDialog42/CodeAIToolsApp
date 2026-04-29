using System;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class SubscriptionDto
    {
        public int? s_id { get; set; }
        public long? u_id { get; set; }
        public string? s_plan { get; set; }  // "free", "premium", "pro"
        public DateTime? s_start_date { get; set; }
        public DateTime? s_end_date { get; set; }
        public bool? s_is_active { get; set; }
        public DateTime? s_created_at { get; set; }
        public DateTime? s_updated_at { get; set; }
    }

    public class SubscriptionPlanDto
    {
        public string? plan_name { get; set; }
        public double? plan_price { get; set; }
        public string? plan_duration { get; set; }  // "monthly", "yearly"
        public string[]? features { get; set; }
    }

    public class CreateSubscriptionDto
    {
        public long? u_id { get; set; }
        public string? s_plan { get; set; }
        public string? payment_method { get; set; }
        public string? payment_token { get; set; }
    }
}
