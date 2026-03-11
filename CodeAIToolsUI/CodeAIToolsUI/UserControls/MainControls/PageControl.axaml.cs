using System;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaEdit.Highlighting;        // Avalonia.AvaloniaEdit paketi
using AvaloniaEdit.Highlighting.Xshd;  // Avalonia.AvaloniaEdit paketi

namespace CodeAIToolsUI.UserControls.MainControls
{
    public partial class PageControl : UserControl
    {
        public PageControl()
        {
            InitializeComponent();
            LoadSyntaxHighlighting();
        }

        private void LoadSyntaxHighlighting()
        {
            var uri = new Uri("avares://CodeAIToolsUI/Java.xshd");
            using var stream = AssetLoader.Open(uri);
            using var reader = new XmlTextReader(stream);
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }
}