using MsBox.Avalonia;
using System.Net.Http;
using Newtonsoft.Json;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;
using System.Collections.Generic;

namespace CodeAIToolsUI.UserControls.MainControls
{
    public partial class WorkerPanelControl : BasePanelControl
    {
        private static readonly HttpClient Http = new();

        public Border SearchInput => SearchInputGrid;
        public Button ButtonGrid => SearchButtonGrid;
        
        public WorkerPanelControl()
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
            ProjectCardsPanel.Children.Clear();

            Http.DefaultRequestHeaders.Clear();

            var userId = RequestManager.ActiveUserDto?.u_id;
            Http.DefaultRequestHeaders.Add("id", userId?.ToString());

            var response = await Http.GetAsync($"{ApiEndpoints.GET_DUTI_API}/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "Failed to retrieve projects.")
                    .ShowAsync();
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<List<ProjectDto>>(content);

            foreach (var project in projects ?? [])
                ProjectCardsPanel.Children.Add(new ProjectCardControl(project));
        }

        protected override async void RefreshButton_Click(object sender, RoutedEventArgs e)
            => await StackProjects();
    }
}