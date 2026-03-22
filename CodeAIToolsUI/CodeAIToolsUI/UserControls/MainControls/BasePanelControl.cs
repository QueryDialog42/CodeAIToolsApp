using System.Text;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Net.Http;
using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;
using MessageBox.Avalonia.Enums;


namespace CodeAIToolsUI.UserControls.MainControls;

public class BasePanelControl : UserControl
{
    public static readonly HttpClient Http = new();

    // ── AXAML Kontrolleri ──────────────────────────────────────────────────────

    private WrapPanel? _projectCardsPanel;
    private Button?    _searchButtonGrid;
    private Border?    _searchInputGrid;

    public WrapPanel ProjectCardsPanel => _projectCardsPanel ??= this.FindControl<WrapPanel>("ProjectCardsPanel")!;
    public Button    SearchButtonGrid  => _searchButtonGrid  ??= this.FindControl<Button>("SearchButtonGrid")!;
    public Border    SearchInputGrid   => _searchInputGrid   ??= this.FindControl<Border>("SearchInputGrid")!;

    // ── Proje Oluşturma ────────────────────────────────────────────────────────

    public async Task HandleCreateProjectRequest(ProjectDto projectDto)
    {
        var json     = JsonConvert.SerializeObject(projectDto);
        var content  = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Http.PostAsync(ApiEndpoints.CRE_PROJ_API, content);

        if (!response.IsSuccessStatusCode)
            await MessageBoxManager
                .GetMessageBoxStandard("Database Failed", $"Failed to save project: {response.StatusCode}", ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
    }

    // ── Virtual Metodlar ───────────────────────────────────────────────────────

    protected virtual void OnLoaded(object? sender, RoutedEventArgs e) { }

    protected virtual Task StackProjects() => Task.CompletedTask;

    protected virtual void RefreshButton_Click(object? sender, RoutedEventArgs e) { }
}