using System;
using System.Diagnostics;
using System.Linq;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Net.Http;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsUI.Views.Items;
using MessageBox.Avalonia.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodeAIToolsUI.UserControls.PopupControl;

namespace CodeAIToolsUI.UserControls.MainControls
{
    public class GitHubUserInfo
    {
        public string? login { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
    }

    public partial class ProjectCardControl : UserControl
    {
        private static readonly HttpClient Http = new();
        public ProjectDto _project { get; set; }
        public ObservableCollection<CollaboratorItem> Collaborators { get; } = new();

        public event EventHandler<ProjectDto>? OpenRequested;
        public event EventHandler<ProjectDto>? DeleteRequested;
        public event EventHandler<ProjectDto>? PullRequested;
        public event EventHandler<ProjectDto>? PushRequested;
        public event EventHandler<ProjectDto>? SaveRequested;

        public ProjectCardControl() : this(new ProjectDto()) { }

        public ProjectCardControl(ProjectDto project)
        {
            InitializeComponent();
            _project = project;
            CollaboratorAvatarList.ItemsSource = Collaborators;
            Populate(project);
            Loaded += async (_, _) => await LoadCollaborators();
        }

        private async void BtnCollaboratorAvatar_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { Tag: CollaboratorItem item }) return;

            var popup = new CollaboratorPopupWindow(item);

            if (RequestManager.ActiveUserDto?.u_role == Configs.ADMIN)
            {
                popup.RemoveRequested += async (_, collaborator) =>
                {
                    await RemoveCollaborator(collaborator);
                    popup.Close();
                };
            }

            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow != null)
                await popup.ShowDialog(parentWindow);
        }

        private async Task RemoveCollaborator(CollaboratorItem item)
        {
            try
            {
                Http.DefaultRequestHeaders.Clear();
                Http.DefaultRequestHeaders.Add("p_id", _project.p_id.ToString());
                Http.DefaultRequestHeaders.Add("w_id", item.UserId.ToString());
                var response = await Http.DeleteAsync(ApiEndpoints.DEL_DUTI_API);

                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to remove collaborator.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                }
                Collaborators.Remove(item);
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException("An error occurred: " + ex.Message);
            }
        }

        #region Project Methods

        private void Populate(ProjectDto project)
        {
            ProjectNameText.Text = project.p_name ?? "İsimsiz Proje";
            ProjectDescText.Text = project.p_description ?? "Açıklama yok.";
            ProjectDateText.Text = project.p_create_time.HasValue
                ? project.p_create_time.Value.ToString("dd.MM.yyyy")
                : "—";
        }

        private async void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the user's GitHub token
                var tokenResponse = await Http.GetAsync(ApiEndpoints.GET_TOK_API + "/" + RequestManager.ActiveUserDto?.u_id);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "GitHub token not found. Please configure your GitHub token.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                var githubToken = await tokenResponse.Content.ReadAsStringAsync();
                githubToken = githubToken?.Trim();

                if (string.IsNullOrEmpty(githubToken))
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "GitHub token not found. Please configure your GitHub token.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                // Get GitHub username using the token
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"token {githubToken}");
                client.DefaultRequestHeaders.Add("User-Agent", "CodeAIToolsApp");

                var userResponse = await client.GetAsync("https://api.github.com/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to get GitHub user information.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<GitHubUserInfo>(userJson);
                
                if (userInfo?.login == null || _project?.p_name == null)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Error", "Unable to construct repository URL.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                // Construct and open the GitHub repository URL
                var repoUrl = $"https://github.com/{userInfo.login}/{_project.p_name}";
                
                if (OperatingSystem.IsWindows())
                    Process.Start(new ProcessStartInfo(repoUrl) { UseShellExecute = true });
                else if (OperatingSystem.IsMacOS())
                    Process.Start(new ProcessStartInfo("open", repoUrl) { UseShellExecute = false });
                else if (OperatingSystem.IsLinux())
                    Process.Start(new ProcessStartInfo("xdg-open", repoUrl) { UseShellExecute = false });
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException("An error occurred while opening repository: " + ex.Message);
            }
        }

        private void PullProject_Click(object sender, RoutedEventArgs e)
            => PullRequested?.Invoke(this, _project);

        private void PushProject_Click(object sender, RoutedEventArgs e)
            => PushRequested?.Invoke(this, _project);

        private void SaveProject_Click(object sender, RoutedEventArgs e)
            => SaveRequested?.Invoke(this, _project);

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
            => DeleteRequested?.Invoke(this, _project);

        #endregion

        #region Collaborator Methods

        private async Task LoadCollaborators()
        {
            var collaborators = await GetProjectCollaborators();
            if (collaborators == null) return;

            Collaborators.Clear();
            foreach (var c in collaborators)
                Collaborators.Add(c);
        }

        private async Task<List<CollaboratorItem>?> GetProjectCollaborators()
        {
            var response = await Http.GetAsync(ApiEndpoints.GET_RESP_API + "/" + _project.p_id);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
    
            var users = JsonConvert.DeserializeObject<List<UserDto>>(json);
            return users?.Select(u => new CollaboratorItem
            {
                UserId   = u.u_id ?? 0,
                Username = u.u_name ?? "",
                GitEmail = u.u_git_email ?? "",
                Role     = u.u_role ?? "—"
            }).ToList();
        }

        #endregion
    }
}