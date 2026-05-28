using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsUI.UserControls.MainControls;
using CodeAIToolsUI.APIs;
using MsBox.Avalonia;
using MessageBox.Avalonia.Enums;
using System.Collections.ObjectModel;

namespace CodeAIToolsUI.Views
{
    public partial class AdminWindow : BaseAppWindow
    {
        protected override BasePanelControl CreatePanelControl() => new AdminPanelControl();
        
        public AdminWindow()
        {
            InitializeComponent();
            InitializeBase();
            
            // Subscribe to subscription updates
            RequestManager.SubscriptionUpdated += (_, _) => UpdateLanguageSelector();
            
            SetupLanguageSelector();
            LoadAiSelectorCore(aiSelector);

            InitializeNotifications();
        }

        private void SetupLanguageSelector()
        {
            var languageSelector = this.FindControl<ComboBox>("languageSelector");
            if (languageSelector != null)
            {
                languageSelector.Loaded += (_, _) => UpdateLanguageSelector();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear current FileTree items
                FileTree.Clear();
                
                // Reload projects
                await LoadProjectsAsync();
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException($"An error occurred while refreshing projects: {ex.Message}");
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
            OpenSettingsDialog();
        }

        private void Subscription_Click(object sender, RoutedEventArgs e)
        {
            new SubscriptionWindow().Show();
        }

        private void InitializeNotifications()
        {
            NotificationList.ItemsSource = _notifications;
            _notifications.CollectionChanged += (_, _) => UpdateBadge();
        }
        public void AddNotification(string message)
        {
            _notifications.Add(message);
        }

        public void RemoveNotification(string message)
        {
            _notifications.Remove(message);
        }

        // Badge'i güncel tutar — doğrudan çağrılmaz
        private void UpdateBadge()
        {
            int count = _notifications.Count;
            NotificationBadge.IsVisible = count > 0;
            NotificationCount.Text = count > 9 ? "9+" : count.ToString();
        }

        private void NotificationButton_Click(object? sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
        }
    }
}