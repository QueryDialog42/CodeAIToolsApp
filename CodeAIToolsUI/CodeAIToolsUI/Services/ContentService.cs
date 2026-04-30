using System;

namespace CodeAIToolsUI.Services
{
    public static class ContentService
    {
        private static string _flowContent = "";
        private static string _codeContent = "";

        public static string FlowContent
        {
            get => _flowContent;
            set => _flowContent = value ?? "";
        }

        public static string CodeContent
        {
            get => _codeContent;
            set => _codeContent = value ?? "";
        }

        public static void UpdateContent(string flowContent, string codeContent)
        {
            FlowContent = flowContent;
            CodeContent = codeContent;
        }
    }
}
