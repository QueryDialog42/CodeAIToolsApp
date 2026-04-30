using System;

namespace CodeAIToolsUI.Services
{
    public static class ContentService
    {
        private static string _flowContent = "";
        private static string _codeContent = "";

        public static event Action<string>? FlowContentChanged;
        public static event Action<string>? CodeContentChanged;

        public static string FlowContent
        {
            get => _flowContent;
            set 
            { 
                _flowContent = value ?? "";
                FlowContentChanged?.Invoke(_flowContent);
            }
        }

        public static string CodeContent
        {
            get => _codeContent;
            set 
            { 
                _codeContent = value ?? "";
                CodeContentChanged?.Invoke(_codeContent);
            }
        }

        public static void UpdateContent(string flowContent, string codeContent)
        {
            FlowContent = flowContent;
            CodeContent = codeContent;
        }
    }
}
