using System;
using Avalonia;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using Avalonia.Media;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Net.Http;
using Avalonia.Controls;
using System.Diagnostics;
using CodeAIToolsUI.APIs;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsUI.Views.Items;
using MessageBox.Avalonia.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Application = Avalonia.Application;
using Avalonia.Controls.ApplicationLifetimes;

namespace CodeAIToolsUI.Views
{
    public partial class NewProjectWindow : Window
    {
        private readonly HttpClient _http = new();

        private bool    _isAdvancedOpen;
        private string? _githubToken;
        private readonly string? _githubUsername = RequestManager.ActiveUserDto?.u_git_email;

        public string ProjectName => TxtProjectName.Text?.Trim() ?? string.Empty;
        public string Description => TxtDescription.Text?.Trim() ?? string.Empty;

        public  ObservableCollection<CollaboratorItem> SelectedCollaborators { get; } = [];
        private ObservableCollection<CollaboratorItem> _allCollaborators              = [];

        public NewProjectWindow()
        {
            InitializeComponent();
            SelectedAvatarList.ItemsSource = SelectedCollaborators;
            CollaboratorList.ItemsSource   = _allCollaborators;

            Loaded += async (_, _) =>
            {
                await InitGithubToken();
                await LoadCollaborators();
            };

            PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };

            CollaboratorList.Tapped += (_, e) =>
            {
                if (e.Source is Control { DataContext: CollaboratorItem item })
                    ToggleSelectCollaborator(item);
            };
        }

        #region Button Methods

        private async Task LoadCollaborators()
        {
            var collaborators = await GetCollaborators();
            if (collaborators == null) return;

            _allCollaborators.Clear();
            foreach (var c in collaborators)
                _allCollaborators.Add(c);
        }

        private void ToggleSelectCollaborator(CollaboratorItem item)
        {
            if (SelectedCollaborators.Any(c => c.Username == item.Username))
                SelectedCollaborators.Remove(item);
            else
                SelectedCollaborators.Add(item);

            RefreshCollaboratorList();
        }

        private void BtnAvatarRemove_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: CollaboratorItem item })
            {
                SelectedCollaborators.Remove(item);
                RefreshCollaboratorList();
            }
        }

        private void RefreshCollaboratorList()
        {
            var query = TxtSearch.Text?.Trim().ToLower() ?? "";

            CollaboratorList.ItemsSource = (
                string.IsNullOrEmpty(query)
                    ? _allCollaborators
                    : _allCollaborators.Where(c =>
                        (c.Username?.ToLower().Contains(query) ?? false) ||
                        (c.GitEmail?.ToLower().Contains(query)  ?? false))
            )
            .Where(c => SelectedCollaborators.All(s => s.Username != c.Username))
            .ToList();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close(false);

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                await ShowWarning("Lütfen bir proje adı girin.");
                TxtProjectName.Focus();
                return;
            }

            SetButtonState(isLoading: true);
            try
            {
                _githubToken = TxtGitHubToken.Text;
                if (await CreateProject())
                {
                    await UpdateIfTokenChanged();
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException("An unexpected error occurred: " + ex.Message);
            }
            finally
            {
                SetButtonState(isLoading: false);
            }
        }

        private void SetButtonState(bool isLoading)
        {
            BtnCreate.IsEnabled = !isLoading;
            BtnCreate.Content   = isLoading ? "Oluşturuluyor..." : "Oluştur";
            BtnCreate.Opacity   = isLoading ? 0.6 : 1.0;
        }

        #endregion

        #region Project Text Methods

        private void TxtProjectName_TextChanged(object sender, TextChangedEventArgs e)
            => UpdatePlaceholder(TxtProjectName, PlaceholderName);

        private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
            => UpdatePlaceholder(TxtDescription, PlaceholderDesc);

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder(TxtSearch, PlaceholderSearch);
            RefreshCollaboratorList();
        }

        private static void UpdatePlaceholder(TextBox textBox, TextBlock placeholder)
            => placeholder.IsVisible = string.IsNullOrEmpty(textBox.Text);

        private async Task<List<CollaboratorItem>?> GetCollaborators()
        {
            var response = await _http.GetAsync($"{ApiEndpoints.GET_COLLS_API}/{RequestManager.ActiveUserDto?.u_id}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserDto>>(content)
                ?.Select(u => new CollaboratorItem
                {
                    UserId   = u.u_id    ?? 0,
                    Username = u.u_name  ?? "",
                    GitEmail = u.u_git_email ?? ""
                }).ToList();
        }

        #endregion

        #region Token Methods

        private void ShowTokenError()
        {
            TxtGitHubToken.BorderThickness = new Thickness(3);
            TxtGitHubToken.BorderBrush     = Brushes.Red;
            TxtTokenError.IsVisible        = true;
            TxtTokenError.Text             = "The GitHub Token is wrong";
        }

        private void ResetTokenError()
        {
            TxtGitHubToken.BorderThickness = new Thickness(1);
            TxtGitHubToken.BorderBrush     = Brushes.Transparent;
            TxtTokenError.IsVisible        = false;
            TxtTokenError.Text             = "";
        }

        private void TxtGitHubToken_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (PlaceholderToken != null)
                PlaceholderToken.IsVisible = string.IsNullOrEmpty(TxtGitHubToken.Text);
        }

        private async Task<string> GetGithubToken()
        {
            if (Design.IsDesignMode || RequestManager.ActiveUserDto == null) return "";

            var response = await _http.GetAsync($"{ApiEndpoints.GET_TOK_API}/{RequestManager.ActiveUserDto.u_id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : "";
        }

        private async Task InitGithubToken()
        {
            var token           = await GetGithubToken();
            _githubToken        = token;
            TxtGitHubToken.Text = token;
        }

        private async Task<bool> IsValidGitHubToken()
        {
            _http.DefaultRequestHeaders.Remove("token");
            _http.DefaultRequestHeaders.Add("token", _githubToken);
            var response = await _http.GetAsync(ApiEndpoints.IS_VAL_API);
            return bool.Parse(await response.Content.ReadAsStringAsync());
        }

        private async Task UpdateIfTokenChanged()
        {
            if (string.IsNullOrEmpty(TxtGitHubToken.Text)) return;

            _http.DefaultRequestHeaders.Remove("token");
            _http.DefaultRequestHeaders.Add("token", _githubToken);
            var response = await _http.GetAsync($"{ApiEndpoints.UPD_TOK_API}/{RequestManager.ActiveUserDto?.u_id}");

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    await GeneralRoutines.ShowException("Git token not found and could not be updated.");
                    break;
                case HttpStatusCode.InternalServerError:
                    await GeneralRoutines.ShowException("Unable to update GitHub Token: " + response.ReasonPhrase);
                    break;
            }
        }

        #endregion

        #region Project Creation Methods

        private async Task<bool> CreateProject()
        {
            var projectDto    = new ProjectDto { p_name = ProjectName, p_description = Description };
            var projectFolder = Path.Combine(BaseAppWindow.RootFolder, projectDto.p_name!);

            ResetTokenError();

            if (!string.IsNullOrEmpty(_githubToken))
            {
                if (!await IsValidGitHubToken())
                {
                    ShowTokenError();
                    return false;
                }
                await CreateGithubRepo(projectDto, projectFolder);
            }

            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.Windows.OfType<BaseAppWindow>().FirstOrDefault()
                ?.LoadProjectsAsync();

            return true;
        }

        public async Task JoinCollabToProject(DutyDto dutyDto)
        {
            var content  = new StringContent(JsonConvert.SerializeObject(dutyDto), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(ApiEndpoints.JOIN_COLL_API, content);

            if (!response.IsSuccessStatusCode)
                await GeneralRoutines.ShowException("An error occured while adding collaborators into project: " + response.ReasonPhrase);
        }

        private static void CreateLocal(string projectFolder)
        {
            Directory.CreateDirectory(projectFolder);
            File.Create(Path.Combine(projectFolder, Configs.FLOW_FILE)).Close();
            File.Create(Path.Combine(projectFolder, Configs.CODE_FILE)).Close();
        }

        private async Task CreateGithubRepo(ProjectDto projectDto, string projectFolder)
        {
            projectDto.belongs_to = RequestManager.ActiveUserDto!.u_id;

            var content = new StringContent(JsonConvert.SerializeObject(projectDto), Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Remove("token");
            _http.DefaultRequestHeaders.Add("token", _githubToken);

            var response = await _http.PostAsync(ApiEndpoints.CRE_GIT_API, content);
            if (response.IsSuccessStatusCode)
                CreateLocal(projectFolder);
        }

        #endregion

        #region UI Methods

        private void BtnAdvancedToggle_Click(object? sender, RoutedEventArgs e)
        {
            _isAdvancedOpen         = !_isAdvancedOpen;
            AdvancedPanel.IsVisible = _isAdvancedOpen;
            AdvancedArrow.Data      = Geometry.Parse(_isAdvancedOpen ? "M18 15l-6-6-6 6" : "M6 9l6 6 6-6");
            SizeToContent           = SizeToContent.Manual;
            SizeToContent           = SizeToContent.Height;
        }

        private void BtnOpenLink_Click(object? sender, RoutedEventArgs e) =>
            Process.Start(new ProcessStartInfo { FileName = ApiEndpoints.TOK_LINK, UseShellExecute = true });

        private static async Task ShowWarning(string message) =>
            await MessageBoxManager
                .GetMessageBoxStandard("Uyarı", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                .ShowAsync();

        #endregion
    }
}