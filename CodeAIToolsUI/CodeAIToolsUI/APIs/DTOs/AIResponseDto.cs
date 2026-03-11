using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CodeAIToolsWPF.APIs.DTOs
{
    public class AIResponseDto
    {
        [JsonProperty("choices")]
        public List<ChoiceDto>? Choices { get; set; }

        public class ChoiceDto
        {
            [JsonProperty("message")]
            public MessageDto? Message { get; set; }
        }

        public class MessageDto
        {
            [JsonProperty("role")]
            public string? Role { get; set; }

            [JsonProperty("content")]
            public string? Content { get; set; }
        }

        public string? GetAssistantContent()
        {
            return Choices?.FirstOrDefault()?.Message?.Content;
        }
    }
}