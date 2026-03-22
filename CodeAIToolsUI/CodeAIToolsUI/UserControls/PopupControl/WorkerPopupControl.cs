using Newtonsoft.Json;
using System.Net.Http;
using CodeAIToolsUI.APIs;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;

namespace CodeAIToolsUI.UserControls.PopupControl
{
    public class WorkerPopupControl : BasePopupControl
    {
        public WorkerPopupControl()
        {
            InitializeComponent();
            SetAdminsVisible();
            Loaded += OnLoaded;
        }

        protected override async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            await CollectAdmins();
        }

        private async Task CollectAdmins()
        {
            AdminList.Children.Clear();

            var userId = RequestManager.ActiveUserDto?.u_id;
            if (userId is null) return;

            try
            {
                var response = await Http.GetAsync(BuildUrl(ApiEndpoints.GET_ADM_API, userId));
                if (!response.IsSuccessStatusCode) return;

                var json          = await response.Content.ReadAsStringAsync();
                var adminCollabs = JsonConvert.DeserializeObject<AdminCollabDto>(json);

                if (adminCollabs is null) return;

                foreach (var admin in adminCollabs.Admins) AddAdminToList(admin);
                foreach (var collabs in adminCollabs.Collaborators) AddCollaboratorToList(collabs);
            }
            catch (HttpRequestException ex)
            {
                await GeneralRoutines.ShowException("An error occured while loading admins: " + ex.Message);
            }
        }
        
        private void AddCollaboratorToList(UserDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.u_name)) return;

            NoCollaboratorsText.IsVisible = false;

            var item = new CollaboratorItemControl(userDto, false);
            
            CollaboratorList.Children.Add(item);
        }

        private void AddAdminToList(UserDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.u_name)) return;

            NoAdminsText.IsVisible = false;

            var item = new CollaboratorItemControl(userDto, true);

            AdminList.Children.Add(item);
        }

        private void SetAdminsVisible()
        {
            AdminStack.IsVisible = true;
        }
    }
}