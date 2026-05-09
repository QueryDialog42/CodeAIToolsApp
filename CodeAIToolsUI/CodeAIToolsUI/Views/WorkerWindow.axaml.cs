using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.Generic;
using CodeAIToolsUI.UserControls.MainControls;
using CodeAIToolsUI.APIs;

namespace CodeAIToolsUI.Views
{
    public partial class WorkerWindow : BaseAppWindow
    {
        protected override BasePanelControl CreatePanelControl() => new WorkerPanelControl();
        
        public WorkerWindow()
        {
            InitializeComponent();
            InitializeBase();
            
            // Subscribe to subscription updates
            RequestManager.SubscriptionUpdated += (_, _) => UpdateLanguageSelector();
            
            SetWorkerAccountPopupStyle();
            SetWorkerPanel();
            SetupLanguageSelector();
            LoadAiSelectorCore(aiSelector);
        }

        private void SetupLanguageSelector()
        {
            var languageSelector = this.FindControl<ComboBox>("languageSelector");
            if (languageSelector != null)
            {
                languageSelector.Loaded += (_, _) => UpdateLanguageSelector();
            }
        }

        private void UpdateLanguageSelector()
        {
            var languageSelector = this.FindControl<ComboBox>("languageSelector");
            if (languageSelector == null) return;

            var user = RequestManager.ActiveUserDto;
            bool hasPremiumOrPro = user?.u_is_subscribed == true && 
                                   user.u_subscription_end > DateTime.Now &&
                                   (user.u_subscription_plan?.ToLower() == "premium" || 
                                    user.u_subscription_plan?.ToLower() == "pro");

            // Enable all options if has Premium/Pro, disable Python and Java if not
            foreach (ComboBoxItem item in languageSelector.Items)
            {
                if (item.Content?.ToString() == "C++")
                {
                    item.IsEnabled = true; // C++ is always available
                }
                else if (item.Content?.ToString() == "Python" || item.Content?.ToString() == "Java")
                {
                    item.IsEnabled = hasPremiumOrPro; // Python and Java require Premium or Pro
                }
            }

            // If current selection is disabled, switch to C++
            if (languageSelector.SelectedItem is ComboBoxItem selectedItem && !selectedItem.IsEnabled)
            {
                foreach (ComboBoxItem item in languageSelector.Items)
                {
                    if (item.IsEnabled && item.Content?.ToString() == "C++")
                    {
                        languageSelector.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Subscription_Click(object sender, RoutedEventArgs e)
        {
            new SubscriptionWindow().Show();
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