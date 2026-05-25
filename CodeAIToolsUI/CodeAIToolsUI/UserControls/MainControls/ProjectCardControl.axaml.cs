using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using CodeAIToolsUI.UserControls.MainControls;
using Avalonia.Media;
using Avalonia.VisualTree;
using CodeAIToolsUI.Services;

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
        public event EventHandler<ProjectDto>? DeniedRequested;

        public ProjectCardControl() : this(new ProjectDto()) { }

        public ProjectCardControl(ProjectDto project)
        {
            InitializeComponent();
            _project = project;
            CollaboratorAvatarList.ItemsSource = Collaborators;
            Populate(project);
            Loaded += async (_, _) => await LoadCollaborators();
            
            // Hide Push and Delete buttons for Worker users
            if (RequestManager.ActiveUserDto?.u_role == Configs.WORKER)
            {
                PushButton.IsVisible = false;
                DeleteButton.IsVisible = false;
                RejectButton.IsVisible = false;
            }
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
            var openButton = sender as Button;
            var originalContent = openButton?.Content;
            
            try
            {
                // Change button text to "opening..." (keep original yellow color)
                if (openButton != null) openButton.Content = new TextBlock { Text = "opening..." };

                // Get the user's GitHub token
                var tokenResponse = await Http.GetAsync(ApiEndpoints.GET_TOK_API + "/" + RequestManager.ActiveUserDto?.u_id);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    if (openButton != null)
                    {
                        openButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    await MessageBoxManager.GetMessageBoxStandard("Error", "GitHub token not found. Please configure your GitHub token.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                var githubToken = await tokenResponse.Content.ReadAsStringAsync();
                githubToken = githubToken?.Trim();

                if (string.IsNullOrEmpty(githubToken))
                {
                    if (openButton != null)
                    {
                        openButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
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
                    if (openButton != null)
                    {
                        openButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to get GitHub user information.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<GitHubUserInfo>(userJson);
                
                if (userInfo?.login == null || _project?.p_name == null)
                {
                    if (openButton != null)
                    {
                        openButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
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
                
                // Change button text to "opened!" with light green color
                if (openButton != null)
                {
                    openButton.Content = new TextBlock { Text = "opened!", Foreground = Avalonia.Media.Brushes.LightGreen };
                }
                
                // Reset to original content after delay
                await Task.Delay(2000);
                if (openButton != null) openButton.Content = originalContent;
            }
            catch (Exception ex)
            {
                if (openButton != null)
                {
                    openButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                }
                await Task.Delay(2000);
                if (openButton != null) openButton.Content = originalContent;
                await GeneralRoutines.ShowException("An error occurred while opening repository: " + ex.Message);
            }
        }

        private async void PullProject_Click(object sender, RoutedEventArgs e)
        {
            var pullButton = sender as Button;
            var originalContent = pullButton?.Content;
            
            try
            {
                if (_project?.p_id == null || _project?.p_name == null)
                {
                    if (pullButton != null)
                    {
                        pullButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    return;
                }

                // Change button text to "pulling..." (keep original blue color)
                if (pullButton != null) pullButton.Content = new TextBlock { Text = "pulling..." };

                // Pull project folder to local CodeAI_Root for sidebar visibility
                var pullFilesUrl = $"{ApiEndpoints.PULL_FILES_API}/{_project.p_id}?projectName={System.Uri.EscapeDataString(_project.p_name)}";
                var pullFilesResponse = await Http.PostAsync(pullFilesUrl, null);

                if (pullFilesResponse.IsSuccessStatusCode)
                {
                    // Change button text to "pulled!" with light green color
                    if (pullButton != null)
                    {
                        pullButton.Content = new TextBlock { Text = "pulled!", Foreground = Avalonia.Media.Brushes.LightGreen };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (pullButton != null) pullButton.Content = originalContent;
                }
                else
                {
                    // Change button text to "Error!" with red color
                    if (pullButton != null)
                    {
                        pullButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (pullButton != null) pullButton.Content = originalContent;
                }
            }
            catch (Exception ex)
            {
                if (pullButton != null)
                {
                    pullButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                }
                await Task.Delay(2000);
                if (pullButton != null) pullButton.Content = originalContent;
            }
        }

        private async void PushProject_Click(object sender, RoutedEventArgs e)
        {
            var pushButton = sender as Button;
            var originalContent = pushButton?.Content;
            
            try
            {
                if (_project?.p_id == null || _project?.p_name == null || RequestManager.ActiveUserDto?.u_id == null)
                {
                    if (pushButton != null)
                    {
                        pushButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    return;
                }

                // Change button text to "pushing..." (keep original purple color)
                if (pushButton != null) pushButton.Content = new TextBlock { Text = "pushing..." };

                // First save the project to ensure latest content is available, then push
                var saveFolderUrl = $"{ApiEndpoints.SAVE_FOLDER_API}/{_project.p_id}?projectName={System.Uri.EscapeDataString(_project.p_name)}";
                var saveResponse = await Http.PostAsync(saveFolderUrl, null);
                
                if (!saveResponse.IsSuccessStatusCode)
                {
                    if (pushButton != null)
                    {
                        pushButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    await Task.Delay(2000);
                    if (pushButton != null) pushButton.Content = originalContent;
                    var saveError = await saveResponse.Content.ReadAsStringAsync();
                    await MessageBoxManager.GetMessageBoxStandard("Save Error", "Failed to save project before pushing: " + saveError, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }
                
                // Now push the saved files from CodeAI_Root to GitHub repository
                var pushFilesUrl = $"{ApiEndpoints.PUSH_FILES_API}/{_project.p_id}?projectName={System.Uri.EscapeDataString(_project.p_name)}&userId={RequestManager.ActiveUserDto.u_id}";
                var pushFilesResponse = await Http.PostAsync(pushFilesUrl, null);

                if (pushFilesResponse.IsSuccessStatusCode)
                {
                    // Change button text to "pushed!" with light green color
                    if (pushButton != null)
                    {
                        pushButton.Content = new TextBlock { Text = "pushed!", Foreground = Avalonia.Media.Brushes.LightGreen };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (pushButton != null) pushButton.Content = originalContent;
                }
                else
                {
                    // Change button text to "Error!" with red color
                    if (pushButton != null)
                    {
                        pushButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (pushButton != null) pushButton.Content = originalContent;
                    
                    // Show error message
                    var errorContent = await pushFilesResponse.Content.ReadAsStringAsync();
                    await MessageBoxManager.GetMessageBoxStandard("Push Error", errorContent, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                if (pushButton != null)
                {
                    pushButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                }
                await Task.Delay(2000);
                if (pushButton != null) pushButton.Content = originalContent;
                await GeneralRoutines.ShowException("An error occurred while pushing to GitHub: " + ex.Message);
            }
        }

        private async void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            var saveButton = sender as Button;
            var originalContent = saveButton?.Content;
            
            try
            {
                if (_project?.p_id == null || _project?.p_name == null)
                {
                    if (saveButton != null)
                    {
                        saveButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    return;
                }

                // Change button text to "saving..." (keep original green color)
                if (saveButton != null) saveButton.Content = new TextBlock { Text = "saving..." };

                // Save entire project folder to home folder
                var saveFolderUrl = $"{ApiEndpoints.SAVE_FOLDER_API}/{_project.p_id}?projectName={System.Uri.EscapeDataString(_project.p_name)}";
                var saveResponse = await Http.PostAsync(saveFolderUrl, null);

                if (saveResponse.IsSuccessStatusCode)
                {
                    // Change button text to "Saved!" with light green color
                    if (saveButton != null)
                    {
                        saveButton.Content = new TextBlock { Text = "Saved!", Foreground = Avalonia.Media.Brushes.LightGreen };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (saveButton != null) saveButton.Content = originalContent;
                }
                else
                {
                    // Change button text to "Error!" with red color
                    if (saveButton != null)
                    {
                        saveButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                    }
                    
                    // Reset to original content after delay
                    await Task.Delay(2000);
                    if (saveButton != null) saveButton.Content = originalContent;
                }
            }
            catch (Exception ex)
            {
                if (saveButton != null)
                {
                    saveButton.Content = new TextBlock { Text = "Error!", Foreground = Avalonia.Media.Brushes.Red };
                }
                await Task.Delay(2000);
                if (saveButton != null) saveButton.Content = originalContent;
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var deleteButton = sender as Button;
            
            // Change button text to "deleting..." with red color
            if (deleteButton != null)
            {
                deleteButton.Content = new TextBlock { Text = "deleting...", Foreground = Avalonia.Media.Brushes.Red };
            }
            
            // Invoke delete request
            DeleteRequested?.Invoke(this, _project);
        }

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

        private async void Denied_Click(object sender, RoutedEventArgs e)
        {

            string denieMessage = $"{ContentService.FlowComment}</@/>{ContentService.CodeComment}";

            var denieDto = new DenieDto
            {
                project_id = _project.p_id,  
                admin_id = RequestManager.ActiveUserDto?.u_id,
                denieReason = denieMessage,
                deniedAt = DateTime.UtcNow
            };

            SetDenieStatusDenieing();
            var http = new HttpClient();


            var json = JsonConvert.SerializeObject(denieDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync(ApiEndpoints.DEN_PROJ, content);

                if (response.IsSuccessStatusCode)
                {
                    SetDenieStatusDenied();
                }
                else
                {
                    SetDenieStatusFailed();
                }
        }

        public void SetDenieStatusDenieing()
        {
            denieTextbox.Text = "Reddediliyor...";
        }

        public void SetDenieStatusDenied()
        {
            denieTextbox.Foreground = Brushes.Lime;
            denieTextbox.Text = "Reddedildi";
        }

        public void SetDenieStatusFailed()
        {
            denieTextbox.Foreground = Brushes.Red;
            denieTextbox.Text = "Reddedilemedi";
        }
        #endregion
    }
}