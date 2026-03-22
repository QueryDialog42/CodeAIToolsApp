using System;
using System.Net;
using System.Linq;
using System.Text;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Net.Http;
using CodeAIToolsUI.Views;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using CodeAIToolsUI.APIs.DTOs;
using MessageBox.Avalonia.Enums;
using static Avalonia.Application;
using static CodeAIToolsUI.GeneralRoutines;
using Avalonia.Controls.ApplicationLifetimes;
using CodeAIToolsUI.UserControls.StartControls;

namespace CodeAIToolsUI.APIs
{
    internal sealed class RequestManager
    {
        public static UserDto? ActiveUserDto;

        private static readonly HttpClient Http = new();

        #region Login Methods

        public static async Task SendLoginRequest(UserDto loginDto, LoginControl loginControl)
        {
            var content = Serialize(loginDto);
            ActiveUserDto = await SetActiveUser(await Http.PostAsync(ApiEndpoints.ACTIV_USER_API, content));
            await HandleLoginResponse(await Http.PostAsync(ApiEndpoints.LOG_API, Serialize(loginDto)), loginControl);
        }

        private static async Task HandleLoginResponse(HttpResponseMessage response, LoginControl loginControl)
        {
            try
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        OpenRelatedWindow();
                        break;
                    case HttpStatusCode.NotFound:
                        ShowError(loginControl.ErrorText, "Email Not Found.");
                        ShowWhereError(loginControl.EmailBox);
                        break;
                    case HttpStatusCode.BadRequest:
                        ShowError(loginControl.ErrorText, "Incorrect Password.");
                        ShowWhereError(loginControl.PasswordBox);
                        break;
                    default:
                        await ShowBox("Error",
                            "Login failed. Maybe server is down. Please try again later: " + response.ReasonPhrase,
                            Icon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                await ShowBox("Unknown Error", "An error occurred: " + ex.Message, Icon.Error);
            }
        }

        private static async Task<UserDto?> SetActiveUser(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return null;
            return JsonConvert.DeserializeObject<UserDto>(await response.Content.ReadAsStringAsync());
        }

        #endregion

        private static void OpenRelatedWindow()
        {
            if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

            var startWindow = desktop.Windows.OfType<StartWindow>().FirstOrDefault();

            switch (ActiveUserDto!.u_role)
            {
                case Configs.ADMIN:  new AdminWindow().Show();  break;
                case Configs.WORKER: new WorkerWindow().Show(); break;
            }

            startWindow?.Close();
        }

        #region Register Methods

        public static async Task SendRegisterRequest(UserDto userDto, RegisterControl registerControl)
        {
            await HandleRegisterResponse(
                await Http.PostAsync(ApiEndpoints.REG_API, Serialize(userDto)),
                registerControl);
        }

        private static async Task HandleRegisterResponse(HttpResponseMessage response, RegisterControl registerControl)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    await ShowBox("Success", "Registration successful! Now please log in.", Icon.Success);

                    if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        desktop.Windows.OfType<StartWindow>().FirstOrDefault()
                            ?.startContent.Content = new LoginControl();
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    ShowError(registerControl.ErrorText, "This email has been taken.");
                    ShowWhereError(registerControl.EmailBox);
                }
            }
            catch (Exception ex)
            {
                await ShowBox("Error", "An error occurred: " + ex.Message, Icon.Error);
            }
        }

        #endregion

        #region Helpers

        private static StringContent Serialize<T>(T obj) =>
            new(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

        private static async Task ShowBox(string title, string message, Icon icon) =>
            await MessageBoxManager
                .GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon)
                .ShowAsync();

        #endregion
    }
}