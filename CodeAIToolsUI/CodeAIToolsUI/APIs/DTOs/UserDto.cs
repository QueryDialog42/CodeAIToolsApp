using System;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class UserDto
    {
        public long? u_id;
        public string? u_name;
        public string? u_email;
        public string? u_git_email;
        public string? u_email_pass;
        public string? u_role;
        public string? u_subscription_plan;  // "free", "premium", "pro"
        public bool? u_is_subscribed;         // true if active subscription
        public DateTime? u_subscription_end;  // subscription end date
    }
}