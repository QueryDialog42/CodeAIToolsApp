using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CodeAIToolsUI.Views
{
    public partial class SettingsWindow : Window
    {
        public string BaseUrl { get; private set; } = string.Empty;
        public string ApiKey  { get; private set; } = string.Empty;

        public SettingsWindow(string currentBaseUrl = "", string currentApiKey = "")
        {
            InitializeComponent();
            BaseUrlBox.Text = currentBaseUrl;
            ApiKeyBox.Text  = currentApiKey;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            BaseUrl = BaseUrlBox.Text?.Trim() ?? string.Empty;
            ApiKey  = ApiKeyBox.Text?.Trim()  ?? string.Empty;
            Close(true);  // result: true = saved
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}