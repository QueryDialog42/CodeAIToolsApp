using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeAIToolsUI.APIs.DTOs;

public class DutyDto
{
    [JsonProperty(PropertyName = "project_name")]
    public required string ProjectName {get; set;}
    
    [JsonProperty(PropertyName = "worker_id")]
    public required List<long> WorkerId {get; set;}
}