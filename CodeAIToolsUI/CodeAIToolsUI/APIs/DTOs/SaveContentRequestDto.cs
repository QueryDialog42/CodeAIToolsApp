using System.Runtime.Serialization;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class SaveContentRequestDto
    {
        public int? projectId { get; set; }
        public string? projectName { get; set; }
        public string? content { get; set; }
    }
}
