using System;
using System.Text;
using Avalonia.Media;
using Newtonsoft.Json;
using System.Net.Http;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;
using System.Collections.Generic;
using CodeAIToolsUI.UserControls.TeamControl;

namespace CodeAIToolsUI.UserControls.PopupControl;

public class AdminPopupControl : BasePopupControl
{
    public AdminPopupControl()
    {
        Loaded += OnLoaded;
    }

    protected override async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await LoadCollaboratorsAsync();
    }

    #region Collaborator Methods

    protected override async Task OnCollaboratorAddClick(Window window)
    {
        try
        {
            var users = await FetchUsersAsync();
            if (users is null) return;

            var searchControl = new CollaboratorSearchControl();
            searchControl.LoadUsers(users);

            window.Content = searchControl;

            searchControl.CollaboratorAdded += async (_, args) =>
            {
                var (user, btn) = args;
                btn.IsEnabled = false;
                btn.Content   = "...";
                await AddCollaboratorToTeamAsync(user, btn);
                AddCollaboratorToList(user);
            };

            window.Show();
            window.Activate();
        }
        catch (Exception ex)
        {
            await GeneralRoutines.ShowException("Could not add collaborator: " + ex.Message);
        }
    }

    private async Task LoadCollaboratorsAsync()
    {
        CollaboratorList.Children.Clear();

        var userId = RequestManager.ActiveUserDto?.u_id;
        if (userId is null) return;

        try
        {
            var response = await Http.GetAsync(BuildUrl(ApiEndpoints.GET_COLLS_API, userId));
            if (!response.IsSuccessStatusCode) return;

            var json          = await response.Content.ReadAsStringAsync();
            var collaborators = JsonConvert.DeserializeObject<List<UserDto>>(json);

            if (collaborators is null) return;

            foreach (var collab in collaborators)
                AddCollaboratorToList(collab);
        }
        catch (HttpRequestException ex)
        {
            await GeneralRoutines.ShowException("An error occured while loading collaborators: " + ex.Message);
        }
    }
    
    private void AddCollaboratorToList(UserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.u_name)) return;

        NoCollaboratorsText.IsVisible = false;

        var item = new CollaboratorItemControl(userDto, false);
        item.RemoveRequested += async (_, _) => await RemoveCollaboratorAsync(userDto, item);
            
        CollaboratorList.Children.Add(item);
    }

    private async Task AddCollaboratorToTeamAsync(UserDto userDto, Button addBtn)
    {
        var userId = RequestManager.ActiveUserDto!.u_id;

        try
        {
            using var content  = BuildJsonContent(userDto);
            var response       = await Http.PostAsync(BuildUrl(ApiEndpoints.ADD_COLL_API, userId), content);

            if (response.IsSuccessStatusCode)
            {
                addBtn.Content = "✓ Eklendi";
            }
            else
            {
                addBtn.Content    = "✕ Hata";
                addBtn.Foreground = new SolidColorBrush(Color.Parse("#f87171"));
                addBtn.Background = new SolidColorBrush(Color.Parse("#1c1c1c"));
            }
        }
        catch (HttpRequestException ex)
        {
            addBtn.Content = "✕ Bağlantı hatası";
            await GeneralRoutines.ShowException("An error occured while adding collaborator: " + ex.Message);
        }
    }
    
    private async Task RemoveCollaboratorAsync(UserDto userDto, Control row)
    {
        var userId = RequestManager.ActiveUserDto?.u_id;
        if (userId is null) return;

        try
        {
            using var content = BuildJsonContent(userDto);
            var response      = await Http.PostAsync(BuildUrl(ApiEndpoints.REM_COLL_API, userId), content);

            if (!response.IsSuccessStatusCode) return;

            CollaboratorList.Children.Remove(row);
            NoCollaboratorsText.IsVisible = CollaboratorList.Children.Count == 0;
        }
        catch (HttpRequestException ex)
        {
            await GeneralRoutines.ShowException("An error occured while removing collaborator: " + ex.Message);
        }
    }

    #endregion

    #region Helpers

    private async Task<List<UserDto>?> FetchUsersAsync()
    {
        try
        {
            var response = await Http.GetAsync(ApiEndpoints.GET_ALL_API + "/" + RequestManager.ActiveUserDto!.u_id);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserDto>>(json);
        }
        catch (HttpRequestException ex)
        {
            await GeneralRoutines.ShowException("An error occured while fetching users: " + ex.Message);
            return null;
        }
    }
    private static StringContent BuildJsonContent<T>(T dto)
        => new(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

    #endregion
    
    
}