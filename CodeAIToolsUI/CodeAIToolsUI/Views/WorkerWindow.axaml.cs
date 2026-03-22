using Avalonia.Media;
using System.Collections.Generic;
using CodeAIToolsUI.UserControls.MainControls;

namespace CodeAIToolsUI.Views
{
    public partial class WorkerWindow : BaseAppWindow
    {
        protected override BasePanelControl CreatePanelControl() => new WorkerPanelControl();
        public WorkerWindow()
        {
            InitializeComponent();
            InitializeBase();
            SetWorkerAccountPopupStyle();
            SetWorkerPanel();
        }

        private void SetWorkerAccountPopupStyle()
        {
            var colors = new Dictionary<string, string>
            {
                ["PopupBg"] = "#202020",
                ["PopupBorder"] = "#2e2e2e",
                ["CardBg"] = "#323232",
                ["DividerColor"] = "#2c2c2c",
                ["TextPrimary"] = "LightGray",
                ["TextMuted"] = "#666666"
            };

            foreach (var (key, value) in colors)
                Resources[key] = new SolidColorBrush(Color.Parse(value));

            PopupContent.AddCollabButton.IsVisible = false;
        }

        private void SetWorkerPanel()
        {
            PanelControl.SearchButtonGrid?.IsVisible = false;
            PanelControl.SearchInputGrid?.IsVisible = false;
        }
    }
}