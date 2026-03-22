using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsUI.Views;
using MessageBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json;

namespace CodeAIToolsUI.UserControls.MainControls;

public partial class AdminPanelControl : BasePanelControl
{
    public AdminPanelControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    protected override async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await StackProjects();
    }

    protected override async Task StackProjects()
    {
        var response = await Http.GetAsync(ApiEndpoints.GET_PROJS_API + "/" + RequestManager.ActiveUserDto?.u_id);
        if (!response.IsSuccessStatusCode) return;

        var json     = await response.Content.ReadAsStringAsync();
        var projects = JsonConvert.DeserializeObject<List<ProjectDto>>(json);
        if (projects == null) return;

        ProjectCardsPanel.Children.Clear();

        foreach (var project in projects)
        {
            var card = new ProjectCardControl(project);
            ProjectCardsPanel.Children.Add(card);
            SubscribeProjectEvents(card);
        }
    }

    private void SubscribeProjectEvents(ProjectCardControl card)
    {
        card.DeleteRequested += async (_, e) =>
            await HandleDeleteProject(PrepareString(e.p_name!), card);
    }
    
    private async void NewProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window mainWindow) return;
 
        var newProjectWindow = new NewProjectWindow();
        var result = await newProjectWindow.ShowDialog<bool>(mainWindow);
 
        if (!result) return;
 
        var projectDto = new ProjectDto
        {
            belongs_to    = RequestManager.ActiveUserDto?.u_id,
            p_name        = newProjectWindow.ProjectName,
            p_description = newProjectWindow.Description,
        };
 
        await HandleCreateProjectRequest(projectDto);
        await newProjectWindow.JoinCollabToProject(new DutyDto
        {
            ProjectName = newProjectWindow.ProjectName,
            WorkerId    = newProjectWindow.SelectedCollaborators.Select(c => c.UserId).ToList()
        });
 
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            (desktop.MainWindow as BaseAppWindow)?.LoadProjectsAsync();
    }

    // ── Delete Flow ────────────────────────────────────────────────────────────

    private async Task HandleDeleteProject(string pName, ProjectCardControl card)
    {
        var gitSuccess = await DeleteFromGit(pName);
        if (gitSuccess)
            await DeleteFromDatabase(pName);

        ProjectCardsPanel.Children.Remove(card);
    }

    private async Task<bool> DeleteFromGit(string pName)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, ApiEndpoints.DEL_GIT_API + Path.VolumeSeparatorChar + pName);
        request.Headers.Add("user_id", RequestManager.ActiveUserDto?.u_id.ToString());
        var response = await Http.SendAsync(request);

        if (response.IsSuccessStatusCode) return true;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Warning", "The repo does not exist already.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                .ShowAsync();
            return true; // repo yok ama DB'den silinebilir
        }

        await MessageBoxManager
            .GetMessageBoxStandard("Git Failed", $"Failed to delete repo: {response.StatusCode}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error)
            .ShowAsync();
        return false;
    }

    private async Task DeleteFromDatabase(string pName)
    {
        var response = await Http.DeleteAsync(ApiEndpoints.DEL_PROJ_API + Path.VolumeSeparatorChar + pName);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Delete Warning", "The project does not exist already.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                .ShowAsync();
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await GeneralRoutines.ShowException("Failed to delete project from database: " + error);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string PrepareString(string projectName)
        => projectName.Trim().Replace(" ", "-");

    protected override async void RefreshButton_Click(object? sender, RoutedEventArgs e)
        => await StackProjects();
}