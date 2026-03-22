using System;
using Avalonia;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.Views;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using CodeAIToolsUI.APIs.DTOs;
using static CodeAIToolsUI.GeneralRoutines;
using Avalonia.Controls.ApplicationLifetimes;

namespace CodeAIToolsUI.UserControls.StartControls
{
    public partial class RegisterControl : UserControl
    {
        public RegisterControl()
        {
            InitializeComponent();
            HandleTextBoxPlaceholder(UserNameBox, UserNamePlaceholder);
            HandleTextBoxPlaceholder(EmailBox, EmailPlaceholder);
            HandleTextBoxPlaceholder(GitEmailBox, GitEmailPlaceholder);
            HandlePasswordBoxPlaceholder(PasswordBox, PasswordPlaceholder);
        }

        #region Login Functions

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // DesignerProperties.GetIsInDesignMode → Design.IsDesignMode
            if (Design.IsDesignMode) return;

            // Window.GetWindow(this) → VisualRoot
            var startWindow = this.VisualRoot as StartWindow;

            if (startWindow == null)
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    startWindow = desktop.MainWindow as StartWindow;
            }

            if (startWindow == null) return;

            startWindow.startContent.Content = new LoginControl();
        }

        #endregion

        #region Register Functions

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = UserNameBox.Text?.Trim() ?? "";
                var email = EmailBox.Text?.Trim() ?? "";
                var gitEmail = GitEmailBox.Text?.Trim() ?? "";
                // Avalonia'da PasswordBox yok; TextBox + PasswordChar kullanılır
                var password = PasswordBox.Text ?? "";

                if (string.IsNullOrEmpty(name))
                {
                    ShowError(ErrorText, "Name can not be empty");
                    ShowWhereError(UserNameBox);
                    return;
                }
                
                if (string.IsNullOrEmpty(email))
                {
                    ShowError(ErrorText, "Email can not be empty");
                    ShowWhereError(EmailBox);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError(ErrorText, "Password can not be empty");
                    ShowWhereError(PasswordBox);
                    return;
                }

                if (!IsValidEmail(email))
                {
                    ShowError(ErrorText, "Invalid Email");
                    ShowWhereError(EmailBox);
                    return;
                }

                switch (password.Length)
                {
                    case < 6:
                        ShowError(ErrorText, "Password should be longer than 6 character");
                        ShowWhereError(PasswordBox);
                        return;
                    
                    case > 12:
                        ShowError(ErrorText, "Password should be shorter than 12 character");
                        ShowWhereError(PasswordBox);
                        return;
                }

                // RadioButton.IsChecked aynı şekilde çalışır Avalonia'da
                var isAdmin = AdminRadioButton.IsChecked == true;

                if (!string.IsNullOrEmpty(gitEmail))
                {
                    if (!await CheckGitHubUserExists(gitEmail)) return;
                }
                else
                {
                    gitEmail = null;
                }

                _ = RequestManager.SendRegisterRequest(new UserDto
                {
                    u_name = name,
                    u_email = email,
                    u_git_email = gitEmail,
                    u_email_pass = password,
                    u_role = isAdmin ? Configs.ADMIN : Configs.WORKER
                }, this);
            }
            catch (Exception ex)
            {
                await ShowException($"Register Error: {ex.Message}");
            }
        }

        private async Task<bool> CheckGitHubUserExists(string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiEndpoints.IS_GIT_EXI_API + Path.VolumeSeparatorChar + username);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ShowError(ErrorText, "There is no account that matches with this Git username.");
                        ShowWhereError(GitEmailBox);
                        return false;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        ShowError(ErrorText, "Unable to reach GitHub. Please check your connection.");
                        ShowWhereError(GitEmailBox);
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                ShowError(ErrorText, "Unable to check Git username. Please try again.");
                return false;
            }
        }

        #endregion
    }
}