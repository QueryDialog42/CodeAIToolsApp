using MsBox.Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using Avalonia.Controls.Primitives;
using System.Text.RegularExpressions;

namespace CodeAIToolsUI
{
    internal class GeneralRoutines
    {
        #region Error Routines

        public static void ShowError(TextBlock textBlock, string message)
        {
            textBlock.Text = message;
            textBlock.IsVisible = true; // Visibility.Visible → IsVisible = true
        }

        public static void ShowWhereError(TemplatedControl control)
        {
            control.Focus(); // Avalonia'da Focus() parametresiz çalışır
            control.BorderBrush = Brushes.Red; // System.Windows.Media → Avalonia.Media
        }

        public static async Task ShowException(string errorText)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", errorText, ButtonEnum.Ok, Icon.Error).ShowAsync();
        }

        #endregion

        #region Email Validation

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        #endregion

        #region Placeholder Routines

        public static void HandleTextBoxPlaceholder(TextBox textBox, TextBlock placeholder)
        {
            textBox.GotFocus += (s, e) => placeholder.IsVisible = false;
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                    placeholder.IsVisible = true;
            };
        }

        // Avalonia'da PasswordBox yoktur; TextBox + PasswordChar kullanılır
        public static void HandlePasswordBoxPlaceholder(TextBox passwordBox, TextBlock placeholder)
        {
            passwordBox.GotFocus += (s, e) => placeholder.IsVisible = false;
            passwordBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(passwordBox.Text))
                    placeholder.IsVisible = true;
            };
        }

        #endregion
    }
}