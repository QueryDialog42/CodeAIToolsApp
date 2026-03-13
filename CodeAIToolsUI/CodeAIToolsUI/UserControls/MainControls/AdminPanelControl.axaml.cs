using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsWPF;
using MessageBox.Avalonia.Enums;

namespace CodeAIToolsUI.UserControls.MainControls
{
    public partial class AdminPanelControl : UserControl
    {
        public AdminPanelControl()
        {
            InitializeComponent();
            Loaded += async (_, _) => await StackProjects();
        }

        #region New Project Methods

        private async Task HandleCreateProjectRequest(ProjectDto projectDto)
        {
            string json = JsonConvert.SerializeObject(projectDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(ApiEndpoints.CRE_PROJ_API, content);
            if (!response.IsSuccessStatusCode)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Database Failed",
                    $"Failed to save project into database: {response.StatusCode}",
                    ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is not Window mainWindow) return;

            var newProjectWindow = new NewProjectWindow();
            var result = await newProjectWindow.ShowDialog<bool>(mainWindow);

            if (result)
            {
                var projectDto = new ProjectDto
                {
                    belongs_to    = RequestManager.activeUserDto?.u_id,
                    p_name        = newProjectWindow.ProjectName,
                    p_description = newProjectWindow.Description,
                };

                _ = HandleCreateProjectRequest(projectDto);
        
                // LoadProjects'i MainWindow'a cast etmeden çağırmak için
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    (desktop.MainWindow as MainWindow)?.LoadProjects();
            }
        }

        #endregion

        #region Unclassified Methods

        private async Task StackProjects()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ApiEndpoints.GET_PROJS_API + Path.VolumeSeparatorChar + RequestManager.activeUserDto?.u_id);

                if (!response.IsSuccessStatusCode) return;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var projects = JsonConvert.DeserializeObject<List<ProjectDto>>(jsonResponse);

                if (projects == null) return;

                ProjectCardsPanel.Children.Clear();

                foreach (var project in projects)
                {
                    var card = new ProjectCardControl(project);
                    ProjectCardsPanel.Children.Add(card);
                    SubscribeProjectEvents(card);
                }
            }
        }

        private void SubscribeProjectEvents(ProjectCardControl card)
        {
            card.DeleteRequested += async (s, e) =>
            {
                var pName = e.p_name;
                using var client = new HttpClient();

                // delete only from repo
                if (await DeleteForGitProject(client, pName!, card)) await DeleteForDataBaseProject(client, pName!, card);

            };
        }

        private async Task<bool> DeleteForGitProject(HttpClient client, string pName, ProjectCardControl card)
        {
            pName = PrepareString(pName);
            
            client.DefaultRequestHeaders.Add("user_id", RequestManager.activeUserDto?.u_id.ToString());
            var response = await client.DeleteAsync(ApiEndpoints.DEL_GIT_API + Path.VolumeSeparatorChar + pName);

            if (response.IsSuccessStatusCode) return true;
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                await MessageBoxManager.GetMessageBoxStandard("Warning", "The repo does not exist already", ButtonEnum.Ok, Icon.Warning).ShowAsync();
                ProjectCardsPanel.Children.Remove(card);
                return false;
            }
            
            var box = MessageBoxManager.GetMessageBoxStandard(
                    "Database Failed",
                    $"Failed to delete project from database: {response.StatusCode}",
                    ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
                return false;
        }

        private async Task DeleteForDataBaseProject(HttpClient client, string pName, ProjectCardControl card)
        {
            var response = await client.DeleteAsync(ApiEndpoints.DEL_PROJ_API + Path.VolumeSeparatorChar + pName);
            if (!response.IsSuccessStatusCode)
            {
                await GeneralRoutines.ShowException("Failed to delete project from database " + response.Content.ReadAsStringAsync().Result);
            }
            ProjectCardsPanel.Children.Remove(card);
        }

        private string PrepareString(string projectName)
        {
            return projectName.Trim().Replace(" ", "-");
        }

        #endregion
    }
}