using Avalonia;
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
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
            HandleTextBoxPlaceholder(EmailBox, EmailPlaceholder);
            HandlePasswordBoxPlaceholder(PasswordBox, PasswordPlaceholder);
        }

        #region Login Functions

        private async Task Authenticate(string email, string password)
        {
            var loginDto = new UserDto
            {
                u_email      = email,
                u_email_pass = password
            };

            _ = RequestManager.SendLoginRequest(loginDto, this);
        }

        #endregion

        #region Button Functions

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email    = EmailBox.Text?.Trim() ?? "";
            string password = PasswordBox.Text ?? ""; // Avalonia'da .Password → .Text

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

            _ = Authenticate(email, password);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
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

            startWindow.startContent.Content = new RegisterControl();
        }

        #endregion
    }
}