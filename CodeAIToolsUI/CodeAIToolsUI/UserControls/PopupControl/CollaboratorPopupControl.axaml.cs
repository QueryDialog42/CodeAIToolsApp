using System;
using Avalonia.Controls;
using System.Diagnostics;
using Avalonia.Interactivity;

using CodeAIToolsUI.Views.Items;

namespace CodeAIToolsUI.UserControls.PopupControl
{
    public partial class CollaboratorPopupWindow : Window
    {
        private readonly CollaboratorItem _item;
        public event EventHandler<CollaboratorItem>? RemoveRequested;

        public CollaboratorPopupWindow(CollaboratorItem item)
        {
            InitializeComponent();
            _item = item;

            TxtInitial.Text  = item.Initial;
            TxtUsername.Text = item.Username;
            TxtRole.Text     = item.Role;
            TxtGitHub.Text   = string.IsNullOrEmpty(item.GitEmail) ? "—" : item.GitEmail;
            BtnProfileLink.IsVisible = !string.IsNullOrEmpty(item.GitEmail);
        }

        private void BtnClose_Click(object? sender, RoutedEventArgs e)
            => Close();

        private void BtnRemove_Click(object? sender, RoutedEventArgs e)
            => RemoveRequested?.Invoke(this, _item);

        private void BtnProfileLink_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_item.GitEmail)) return;
            Process.Start(new ProcessStartInfo
            {
                FileName        = $"https://github.com/{_item.GitEmail}",
                UseShellExecute = true
            });
        }
    }
}