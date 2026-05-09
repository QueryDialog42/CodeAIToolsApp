using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs;

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

            Console.WriteLine(currentBaseUrl);
            Console.WriteLine(currentApiKey);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            BaseUrl = BaseUrlBox.Text?.Trim() ?? string.Empty;
            ApiKey  = ApiKeyBox.Text?.Trim()  ?? string.Empty;

            var Http = new HttpClient();
            var jsonContent = $"[\"{BaseUrl}\", \"{ApiKey}\"]";
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = Http.PostAsync(ApiEndpoints.SAV_BAS_URL_AND_KEY_API + RequestManager.ActiveUserDto?.u_id, content).Result;
            if (!response.IsSuccessStatusCode)
            {
                await GeneralRoutines.ShowException(response.ReasonPhrase);
                return;
            }

            Close(true);  // result: true = saved
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}