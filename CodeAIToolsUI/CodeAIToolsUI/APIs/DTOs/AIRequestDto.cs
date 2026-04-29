using System.Collections.Generic;

namespace CodeAIToolsUI.APIs.DTOs
{
    public class AIRequestDto
    {
        public string? model { get; set; }
        public List<MessageDto>? messages = new List<MessageDto>();
        public double temperature { get; set; }
        public double max_tokens { get; set; }
        public double top_p { get; set; }
        public double frequency_penalty { get; set; }
        public double presence_penalty { get; set; }
        public string languageToParse { get; set; }

        public class MessageDto
        {
            public string? role { get; set; } = null;

            public string? content { get; set; }
        }

        public AIRequestDto(string flowPageLines)
        {
            messages.Add(new MessageDto { content = flowPageLines });
        }
    }
}