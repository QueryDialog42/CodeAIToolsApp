using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsWPF;
using MessageBox.Avalonia.Enums;
using MsBox.Avalonia;
using Octokit;
using LibGit2Sharp;

namespace CodeAIToolsUI
{
    public partial class NewProjectWindow : Window
    {
        private bool _isAdvancedOpen = false;
        private string? github_token;
        private string? github_username = RequestManager.activeUserDto.u_git_email;
        
        public NewProjectWindow()
        {
            InitializeComponent();
            // DragMove → BeginMoveDrag
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
            github_token = TxtGitHubToken?.Text;
            TxtTokenError.IsVisible = false;
            TxtTokenError.Text = "";
            
            if (!string.IsNullOrEmpty(github_token))
            {
                if (await IsValidGitHubToken(github_token))
                {
                    // 2. GitHub'da repository oluştur
            
                    string repoUrl = await CreateGitHubRepository(projectDto);

                    // 3. Yerel Git repo'yu başlat ve ilk commit'i at
                    if (!string.IsNullOrEmpty(repoUrl))
                    {
                        InitLocalGitRepo(projectFolder, repoUrl);
                    }
                }
                else
                {
                    showTokenError();
                    return false;
                }
            }
            
            var flowText = Path.Combine(projectFolder, Configs.FLOW_FILE);
            var codeText = Path.Combine(projectFolder, Configs.CODE_FILE);

            // 1. Yerel klasörü ve dosyaları oluştur
            Directory.CreateDirectory(projectFolder);
            File.Create(flowText).Close();
            File.Create(codeText).Close();
            
            return true;
        }

        private async Task<string> CreateGitHubRepository(ProjectDto projectDto)
        {
            try
            {
                var client = new GitHubClient(new Octokit.ProductHeaderValue("CodeAIToolsProject"));
                client.Credentials = new Octokit.Credentials(github_token);

                var newRepo = new NewRepository(projectDto.p_name)
                {
                    Description = projectDto.p_description,
                    Private = false,
                    AutoInit = false
                };

                var repo = await client.Repository.Create(newRepo);
                return repo.CloneUrl;
            }
            catch (Exception ex)
            {
                GeneralRoutines.ShowException($"An error occured while creating GitHub repository: {ex.Message}");
                return null;
            }
        }

        private void InitLocalGitRepo(string projectFolder, string remoteUrl)
        {
            // Git repo başlat
            LibGit2Sharp.Repository.Init(projectFolder);

            using var repo = new LibGit2Sharp.Repository(projectFolder);

            // Remote ekle
            repo.Network.Remotes.Add("origin", remoteUrl);

            // Tüm dosyaları stage'e al
            Commands.Stage(repo, "*");

            // İlk commit
            var signature = new LibGit2Sharp.Signature(github_username, "you@email.com", DateTimeOffset.Now);
            repo.Commit("Initial commit", signature, signature);

            // Push
            var options = new PushOptions
            {
                CredentialsProvider = (url, user, cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = github_username,
                        Password = github_token
                    }
            };

            repo.Network.Push(repo.Branches["main"], options);
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

        private async Task<bool> IsValidGitHubToken(string token)
        {
            try
            {
                var client = new GitHubClient(new Octokit.ProductHeaderValue("CodeAIToolsIsTokenValid"));
                client.Credentials = new Octokit.Credentials(token);

                // Kendi kullanıcı bilgini çek, başarılıysa token geçerli
                var user = await client.User.Current();
                return user != null;
            }
            catch (AuthorizationException)
            {
                return false; // Token geçersiz veya yetkisiz
            }
            catch (Exception)
            {
                return false; // Bağlantı hatası vb.
            }
        }

        #endregion

        private void showTokenError()
        {
            TxtGitHubToken?.BorderThickness = new Thickness(3);
            TxtGitHubToken?.BorderBrush = Brushes.Red;
            TxtTokenError.IsVisible = true;
            TxtTokenError.Text = "The Github Token is wrong";
        }
    }
}