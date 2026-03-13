using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsWPF;
using MessageBox.Avalonia.Enums;
using MsBox.Avalonia;
using Octokit;
using LibGit2Sharp;
using Newtonsoft.Json;
using Application = Avalonia.Application;

namespace CodeAIToolsUI
{
    public partial class NewProjectWindow : Window
    {
        private bool _isAdvancedOpen = false;
        private string? _githubToken;
        private readonly string? _githubUsername = RequestManager.activeUserDto?.u_git_email;
        
        public NewProjectWindow()
        {
            InitializeComponent();
            Loaded += async (_, _) => await initGithubToken();
            PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };
        }

        public string AramaMevcutu => TxtSearch.Text?.Trim() ?? "";
        public string ProjectName  => TxtProjectName.Text?.Trim() ?? "";
        public string Description  => TxtDescription.Text?.Trim() ?? "";

        #region Button Methods

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false); // DialogResult = false
        }

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Uyarı", "Lütfen bir proje adı girin.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                await box.ShowAsync();
                TxtProjectName.Focus();
                return;
            }

            var projectDto = new ProjectDto
            {
                p_name = ProjectName,
                p_description = Description
            };

            _githubToken = TxtGitHubToken.Text;

            if (await CreateProject(projectDto)) Close(true); // DialogResult = true
        }

        #endregion

        #region Collaborator Methods

        private async void BtnAddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "İşbirlikçi Ekle", "İşbirlikçi ekleme açılacak.",
                ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowAsync();
        }

        private async void BtnRemoveCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "İşbirlikçi Kaldır", "İşbirlikçi kaldırma açılacak.",
                ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowAsync();
        }

        #endregion

        #region Project Text Methods

        private void TxtProjectName_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderName.IsVisible = string.IsNullOrEmpty(TxtProjectName.Text);

        private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderDesc.IsVisible = string.IsNullOrEmpty(TxtDescription.Text);

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderSearch.IsVisible = string.IsNullOrEmpty(TxtSearch.Text);

        #endregion

        #region Unclassified Methods

        private async Task<bool> CreateProject(ProjectDto projectDto) {
            var projectFolder = Path.Combine(MainWindow.RootFolder, projectDto.p_name!);
            _githubToken = TxtGitHubToken?.Text;
            TxtTokenError.IsVisible = false;
            TxtTokenError.Text = "";
            
            if (!string.IsNullOrEmpty(_githubToken))
            {
                if (await IsValidGitHubToken())
                {
                    await CreateGithubRepo(projectDto, projectFolder);
                }
                else
                {
                    ShowTokenError();
                    return false;
                }
            }
            
            
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Windows.OfType<MainWindow>().FirstOrDefault()?.LoadProjects();
            }
            
            return true;
        }
        
        private void BtnAdvancedToggle_Click(object? sender, RoutedEventArgs e)
        {
            _isAdvancedOpen = !_isAdvancedOpen;
            AdvancedPanel.IsVisible = _isAdvancedOpen;

            AdvancedArrow.Data = Geometry.Parse(_isAdvancedOpen
                ? "M18 15l-6-6-6 6"
                : "M6 9l6 6 6-6");

            // Pencereyi içeriğe göre yeniden boyutlandır
            this.SizeToContent = SizeToContent.Manual;
            this.SizeToContent = SizeToContent.Height;
        }

        private void TxtGitHubToken_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (PlaceholderToken != null)
                PlaceholderToken.IsVisible = string.IsNullOrEmpty(TxtGitHubToken.Text);
        }

        #endregion

        private void ShowTokenError()
        {
            TxtGitHubToken?.BorderThickness = new Thickness(3);
            TxtGitHubToken?.BorderBrush = Brushes.Red;
            TxtTokenError.IsVisible = true;
            TxtTokenError.Text = "The Github Token is wrong";
        }

        private void CreateLocal(string projectFolder)
        {
            var flowText = Path.Combine(projectFolder, Configs.FLOW_FILE);
            var codeText = Path.Combine(projectFolder, Configs.CODE_FILE);

            // 1. Yerel klasörü ve dosyaları oluştur
            Directory.CreateDirectory(projectFolder);
            File.Create(flowText).Close();
            File.Create(codeText).Close();
        }

        private async Task CreateGithubRepo(ProjectDto projectDto, string projectFolder)
        {
            projectDto.belongs_to = RequestManager.activeUserDto!.u_id;
            
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(projectDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("token", _githubToken); 
                
                var response = await client.PostAsync(ApiEndpoints.CRE_GIT_API, content);
                if (response.IsSuccessStatusCode)
                {
                    CreateLocal(projectFolder);
                }
            }
        }

        private async Task<bool> IsValidGitHubToken()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("token", _githubToken); 
                var response = await client.GetAsync(ApiEndpoints.IS_VAL_API);
                var isValid = bool.Parse(await response.Content.ReadAsStringAsync());
                return isValid;
            }
        }

        private async Task<string> GetGithubToken()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ApiEndpoints.GET_TOK_API + Path.VolumeSeparatorChar + RequestManager.activeUserDto!.u_id);
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                return "";
            }
        }

        private async Task initGithubToken()
        {
            string token = await GetGithubToken();
            _githubToken = token;
            TxtGitHubToken.Text = token;
        }
        
        private void BtnOpenLink_Click(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ApiEndpoints.TOK_LINK,
                UseShellExecute = true
            });
        }
    }
}