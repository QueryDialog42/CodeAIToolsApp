using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CodeAIToolsWPF;
using CodeAIToolsWPF.APIs.DTOs;

namespace CodeAIToolsUI.UserControls.TeamControl
{
    public partial class CollaboratorSearchControl : UserControl
    {
        public event EventHandler? CloseRequested;
        public event EventHandler<(UserDto user, Button btn)>? CollaboratorAdded;

        private List<UserDto> _allUsers = new();

        public CollaboratorSearchControl()
        {
            InitializeComponent();
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
                Background   = new SolidColorBrush(Color.Parse(isAdmin ? "#1d4ed8" : "#065f46"))
            };
            var avatarText = new TextBlock
            {
                Text                = user.u_email?.Length > 0 ? user.u_email[0].ToString().ToUpper() : "?",
                Foreground          = Brushes.White,
                FontSize            = 14,
                FontWeight          = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };
            avatar.Child = avatarText;

            // Info column
            var infoPanel = new StackPanel
            {
                Margin            = new Thickness(10, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var emailText = new TextBlock
            {
                Text         = user.u_email ?? "—",
                Foreground   = new SolidColorBrush(Color.Parse("#f1f5f9")),
                FontSize     = 12,
                FontWeight   = FontWeight.Medium,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoPanel.Children.Add(emailText);

            if (!string.IsNullOrEmpty(user.u_git_email))
            {
                var gitEmail = new TextBlock
                {
                    Text         = $"🐱 {user.u_git_email}",
                    Foreground   = new SolidColorBrush(Color.Parse("#6b7280")),
                    FontSize     = 10,
                    TextTrimming = TextTrimming.CharacterEllipsis,
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
                Background   = new SolidColorBrush(Color.Parse(isAdmin ? "#1e3a8a" : "#064e3b"))
            };
            var roleText = new TextBlock
            {
                Text       = isAdmin ? "🛡️ Admin" : "👷 Worker",
                Foreground = Brushes.White,
                FontSize   = 10,
                FontWeight = FontWeight.SemiBold
            };
            roleBadge.Child = roleText;
            rolePanel.Children.Add(roleBadge);
            infoPanel.Children.Add(rolePanel);

            // Add button — FindResource yerine Classes kullan
            var addBtn = new Button
            {
                Content           = "+ Ekle",
                VerticalAlignment = VerticalAlignment.Center,
                Classes           = { "addBtn" }
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
                EmptyText.IsVisible   = true;
                NotFoundText.IsVisible = false;
                ResultsList.Children.Clear();
                return;
            }

            var filtered = _allUsers
                .Where(u => (u.u_email?.ToLower().Contains(query) ?? false) ||
                            (u.u_git_email?.ToLower().Contains(query) ?? false))
                .ToList();

            EmptyText.IsVisible    = false;
            NotFoundText.IsVisible = filtered.Count == 0;

            RenderUsers(filtered);
        }

        #endregion
    }
}