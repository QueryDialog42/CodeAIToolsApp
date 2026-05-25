using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class DenieDto
    {
        public int? project_id {get; set;}
        public long? admin_id {get; set;}
        public string? denieReason {get; set;}
        public DateTime deniedAt {get; set;}
    }
}