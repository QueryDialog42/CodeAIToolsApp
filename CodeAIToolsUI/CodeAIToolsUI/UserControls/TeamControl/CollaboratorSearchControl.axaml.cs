
using System;
using Avalonia;
using System.Linq;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;
using System.Collections.Generic;


namespace CodeAIToolsUI.UserControls.TeamControl
{
    public partial class CollaboratorSearchControl : UserControl
    {
        private List<UserDto> _allUsers = new();
        public event EventHandler? CloseRequested;
        public event EventHandler<(UserDto user, Button btn)>? CollaboratorAdded;

        public CollaboratorSearchControl()
        {
            InitializeComponent();
            CloseRequested += (_, _) => IsVisible = false;
        }

        #region User Loading Methods

        public void LoadUsers(List<UserDto> users)
        {
            _allUsers = users;
            RenderUsers(_allUsers);
        }

        private Border BuildUserCard(UserDto user)
        {
            var card = new Border
            {
                Background   = new SolidColorBrush(Color.Parse("#22262e")),
                CornerRadius = new CornerRadius(8),
                Padding      = new Thickness(12, 10, 12, 10),
                Margin       = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Avatar
            bool isAdmin = user.u_role?.ToUpper() == Configs.ADMIN;
            var avatar = new Border
            {
                Width        = 36,
                Height       = 36,
                CornerRadius = new CornerRadius(18),
                Background   = new SolidColorBrush(Color.Parse(isAdmin ? Marks.ADM_COL : Marks.WORK_COL))
            };
            var avatarText = new TextBlock
            {
                FontSize            = 14,
                FontWeight          = FontWeight.Bold,
                Foreground          = Brushes.White,
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = user.u_name?.Length > 0 ? user.u_name[0].ToString().ToUpper() : "?"
                
            };
            avatar.Child = avatarText;

            // Info column
            var infoPanel = new StackPanel
            {
                Margin            = new Thickness(10, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var userText = new TextBlock
            {
                FontSize     = 12,
                FontWeight   = FontWeight.Medium,
                Text         = user.u_name ?? "—",
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground   = new SolidColorBrush(Color.Parse("#f1f5f9")),
                
                
            };
            infoPanel.Children.Add(userText);

            if (!string.IsNullOrEmpty(user.u_git_email))
            {
                var gitEmail = new TextBlock
                {
                    FontSize     = 10,
                    Text         = $"🐱 {user.u_git_email}",
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground   = new SolidColorBrush(Color.Parse("#6b7280")),
                    Margin       = new Thickness(0, 2, 0, 0)
                };
                infoPanel.Children.Add(gitEmail);
            }

            // Role badge
            var rolePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0)
            };
            var roleBadge = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding      = new Thickness(6, 2, 6, 2),
                Background   = new SolidColorBrush(Color.Parse(isAdmin ? Marks.ADM_DRK_COL : Marks.WORK_DRK_COL)),
            };
            var roleText = new TextBlock
            {
                FontSize   = 10,
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold,
                Text       = isAdmin ? "🛡️ Admin" : "👷 Worker"
            };
            roleBadge.Child = roleText;
            rolePanel.Children.Add(roleBadge);
            infoPanel.Children.Add(rolePanel);

            // Add button — FindResource yerine Classes kullan
            var addBtn = new Button
            {
                Content           = "+ Ekle",
                Classes           = { "addBtn" },
                VerticalAlignment = VerticalAlignment.Center
            };
            addBtn.Click += (_, _) =>
            {
                CollaboratorAdded?.Invoke(this, (user, addBtn));
            };

            Grid.SetColumn(avatar,    0);
            Grid.SetColumn(infoPanel, 1);
            Grid.SetColumn(addBtn,    2);

            grid.Children.Add(avatar);
            grid.Children.Add(infoPanel);
            grid.Children.Add(addBtn);

            card.Child = grid;
            return card;
        }

        private void RenderUsers(List<UserDto> users)
        {
            ResultsList.Children.Clear();
            foreach (var user in users)
                ResultsList.Children.Add(BuildUserCard(user));
        }

        #endregion

        #region Unclassified Methods

        private void Close_Click(object sender, RoutedEventArgs e)
            => CloseRequested?.Invoke(this, EventArgs.Empty);

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text?.Trim().ToLower() ?? "";

            // Visibility → IsVisible
            SearchPlaceholder.IsVisible = string.IsNullOrEmpty(SearchBox.Text);

            if (string.IsNullOrEmpty(query))
            {
                EmptyText.IsVisible    = false;
                NotFoundText.IsVisible = false;
                RenderUsers(_allUsers);  // Tüm kullanıcıları göster
                return;
            }

            var filtered = _allUsers
                .Where(u => (u.u_name?.ToLower().Contains(query) ?? false) ||
                            (u.u_git_email?.ToLower().Contains(query) ?? false))
                .ToList();

            EmptyText.IsVisible    = false;
            NotFoundText.IsVisible = filtered.Count == 0;

            RenderUsers(filtered);
        }

        #endregion
    }
}