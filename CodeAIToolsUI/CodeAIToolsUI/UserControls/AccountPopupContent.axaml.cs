using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Newtonsoft.Json;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.UserControls.TeamControl;
using CodeAIToolsWPF;
using CodeAIToolsWPF.APIs.DTOs;

namespace CodeAIToolsUI.UserControls
{
    public partial class AccountPopupContent : UserControl
    {
        public event EventHandler? CloseRequested;
        public event EventHandler? LogoutRequested;

        public AccountPopupContent()
        {
            InitializeComponent();
            Loaded += async (_, _) => await CollectCollaborators();
        }

        #region Button Event Handlers

        private void Close_Click(object sender, RoutedEventArgs e)
            => CloseRequested?.Invoke(this, EventArgs.Empty);

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            // TODO: profil düzenleme sayfasına git
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            RequestManager.activeUserDto = null;
            LogoutRequested?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Collaborator Management Methods

        public void AddCollaboratorToList(UserDto userDto)
        {
            string? email = userDto.u_email;
            if (string.IsNullOrEmpty(email)) return;

            // Visibility.Collapsed → IsVisible = false
            NoCollaboratorsText.IsVisible = false;

            var row = new Grid { Margin = new Thickness(4, 3, 4, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var emailText = new TextBlock
            {
                Text = email,
                // ColorConverter.ConvertFromString → Color.Parse
                Foreground = new SolidColorBrush(Color.Parse("#f1f5f9")),
                FontSize = 11,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            // FindResource → kaynak class üzerinden ya da doğrudan atama
            var removeBtn = new Button { Classes = { "removeBtn" } };
            removeBtn.Content = new TextBlock
            {
                Text = "✕",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.Parse("#f87171"))
            };
            removeBtn.Click += async (_, _) =>
            {
                await RemoveCollaboratorFromTeam(userDto, row);
            };

            Grid.SetColumn(emailText, 0);
            Grid.SetColumn(removeBtn, 1);
            row.Children.Add(emailText);
            row.Children.Add(removeBtn);
            CollaboratorList.Children.Add(row);
        }

        private async void AddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var searchControl = new CollaboratorSearchControl();
            var users = await CollectUsers();
            searchControl.LoadUsers(users);

            var window = new Window
            {
                Content = searchControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                // WindowStyle.None → SystemDecorations.None
                SystemDecorations = SystemDecorations.None,
                // AllowsTransparency + Background Transparent → TransparencyLevelHint
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                Background = Brushes.Transparent,
                CanResize = false,
                ShowInTaskbar = false,
                Topmost = true
            };

            searchControl.CollaboratorAdded += async (_, args) =>
            {
                var (user, btn) = args;
                btn.IsEnabled = false;
                btn.Content = "...";
                await AddCollaboratorToTeam(user, btn);
                AddCollaboratorToList(user);
            };

            searchControl.CloseRequested += (_, _) => window.Close();

            window.Show();
            window.Activate(); // Focus() → Activate()
        }

        private async Task CollectCollaborators()
        {
            CollaboratorList.Children.Clear();

            using var client = new HttpClient();
            var response = await client.GetAsync(
                ApiEndpoints.GET_COLLS_API + $"/{RequestManager.activeUserDto?.u_id}");

            if (!response.IsSuccessStatusCode) return;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var collaborators = JsonConvert.DeserializeObject<List<UserDto>>(jsonResponse);

            if (collaborators == null) return;

            foreach (var collab in collaborators)
                AddCollaboratorToList(collab);
        }

        private async Task AddCollaboratorToTeam(UserDto userDto, Button addBtn)
        {
            string json = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(
                ApiEndpoints.ADD_COLL_API + $"/{RequestManager.activeUserDto?.u_id}", content);

            if (response.IsSuccessStatusCode)
            {
                addBtn.Content = "✓ Eklendi";
            }
            else
            {
                addBtn.Content = "✕ Hata";
                addBtn.Foreground = new SolidColorBrush(Color.Parse("#f87171"));
                addBtn.Background = new SolidColorBrush(Color.Parse("#f1f5f9"));
            }
        }

        private async Task RemoveCollaboratorFromTeam(UserDto userDto, Grid row)
        {
            string json = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(
                ApiEndpoints.REM_COLL_API + $"/{RequestManager.activeUserDto?.u_id}", content);

            if (response.IsSuccessStatusCode)
            {
                CollaboratorList.Children.Remove(row);
                if (CollaboratorList.Children.Count == 0)
                    NoCollaboratorsText.IsVisible = true;
            }
        }

        #endregion

        #region Unclassified Methods

        public void SetInfo(string displayName, string email,
                            string? githubEmail = null, bool isAdmin = false)
        {
            AvatarInitials.Text = displayName.Length > 0
                ? displayName[0].ToString().ToUpper()
                : "U";
            DisplayName.Text = displayName;
            EmailText.Text = email;

            if (!string.IsNullOrWhiteSpace(githubEmail))
            {
                GithubEmailText.Text = githubEmail;
                GithubCard.IsVisible = true;
            }
            else
            {
                GithubCard.IsVisible = false;
            }

            if (isAdmin)
            {
                RoleLabel.Text = char.ToUpper(Configs.ADMIN[0]) + Configs.ADMIN[1..].ToLower();
                RoleIcon.Text = Icons.ADM_ICON;
                RoleBadge.Background = new SolidColorBrush(Color.Parse(Marks.ADM_POP_TITLE_COL));
            }
            else
            {
                RoleLabel.Text = char.ToUpper(Configs.WORKER[0]) + Configs.WORKER[1..].ToLower();
                RoleIcon.Text = Icons.WORK_ICON;
                RoleBadge.Background = new SolidColorBrush(Color.Parse(Marks.WORK_POP_TITLE_COL));
            }
        }

        private async Task<List<UserDto>> CollectUsers()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(
                ApiEndpoints.GET_ALL_API + $"/{RequestManager.activeUserDto?.u_id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UserDto>>(jsonResponse)
                       ?? new List<UserDto>();
            }
            return new List<UserDto>();
        }

        #endregion
    }
}