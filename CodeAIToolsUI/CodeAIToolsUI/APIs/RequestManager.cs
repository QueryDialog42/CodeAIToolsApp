using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using CodeAIToolsWPF;
using CodeAIToolsWPF.APIs.DTOs;
using CodeAIToolsUI.UserControls.StartControls;
using MessageBox.Avalonia.Enums;
using static CodeAIToolsUI.GeneralRoutines;

namespace CodeAIToolsUI.APIs
{
    class RequestManager
    {
        public static UserDto? activeUserDto;

        #region Login Methods

        public static async Task SendLoginRequest(UserDto logindto, LoginControl loginControl)
        {
            string json = JsonConvert.SerializeObject(logindto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            activeUserDto = await SetActiveUser(await client.PostAsync(ApiEndpoints.ACTIV_USER_API, content));
            await HandleLoginResponse(await client.PostAsync(ApiEndpoints.LOG_API, content), loginControl);
        }

        private static async Task HandleLoginResponse(HttpResponseMessage response, LoginControl loginControl)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Application.Current.Windows → ApplicationLifetime üzerinden
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var startWindow = desktop.Windows.OfType<StartWindow>().FirstOrDefault();
                        new MainWindow().Show();
                        startWindow?.Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ShowError(loginControl.ErrorText, "Email Not Found.");
                    ShowWhereError(loginControl.EmailBox);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    ShowError(loginControl.ErrorText, "Incorrect Password.");
                    ShowWhereError(loginControl.PasswordBox);
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        "Login failed. Maybe server is down. Please try again later: " + response.ReasonPhrase,
                        ButtonEnum.Ok, Icon.Error);
                    await box.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Unknown Error",
                    "An error occurred: " + ex.Message,
                    ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }
        }

        private static async Task<UserDto?> SetActiveUser(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(jsonResponse);
            }
            return null;
        }

        #endregion

        #region Register Methods

        public static async Task SendRegisterRequest(UserDto requestdto, RegisterControl registerControl)
        {
            string json = JsonConvert.SerializeObject(requestdto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            await HandleRegisterResponse(await client.PostAsync(ApiEndpoints.REG_API, content), registerControl);
        }

        private static async Task HandleRegisterResponse(HttpResponseMessage response, RegisterControl registerControl)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Success",
                        "Registration successful! Now please log in.",
                        ButtonEnum.Ok, Icon.Success);
                    await box.ShowAsync();

                    // Application.Current.Windows → ApplicationLifetime üzerinden
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var startWindow = desktop.Windows.OfType<StartWindow>().FirstOrDefault();
                        if (startWindow != null)
                            startWindow.startContent.Content = new LoginControl();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    ShowError(registerControl.ErrorText, "This email has been taken.");
                    ShowWhereError(registerControl.EmailBox);
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    "An error occurred: " + ex.Message,
                    ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }
        }

        #endregion
    }
}