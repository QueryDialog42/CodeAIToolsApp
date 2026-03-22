using System;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class ProjectDto
    {
        public int? p_id {  get; set; }
        public long? belongs_to {  get; set; }
        public string? p_name { get; set; }
        public double? p_size { get; set; }
        public string? p_description { get; set; }
        public DateTime? p_create_time { get; set; }
        public DateTime? p_update_time { get; set; }
    }
}