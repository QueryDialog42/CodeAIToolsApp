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
        public static event EventHandler? SubscriptionUpdated;

        private static readonly HttpClient Http = new();

        #region Login Methods

        public static async Task SendLoginRequest(UserDto loginDto, LoginControl loginControl)
        {
            var content = Serialize(loginDto);
            ActiveUserDto = await SetActiveUser(await Http.PostAsync(ApiEndpoints.ACTIV_USER_API, content));
            
            // Load subscription data if user login is successful
            if (ActiveUserDto != null)
            {
                await LoadUserSubscriptionData();
            }
            
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

        #region Subscription Methods

        public static async Task<SubscriptionDto?> GetUserSubscriptionAsync(long userId)
        {
            try
            {
                var response = await Http.GetAsync($"{ApiEndpoints.GET_SUBSCRIPTION_API}?user_id={userId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SubscriptionDto>(responseBody);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting subscription: {ex.Message}");
            }
            return null;
        }

        public static async Task<bool> CreateSubscriptionAsync(CreateSubscriptionDto subscriptionDto)
        {
            try
            {
                var content = Serialize(subscriptionDto);
                var response = await Http.PostAsync(ApiEndpoints.CREATE_SUBSCRIPTION_API, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating subscription: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CancelSubscriptionAsync(long userId)
        {
            try
            {
                var response = await Http.DeleteAsync($"{ApiEndpoints.CANCEL_SUBSCRIPTION_API}?user_id={userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling subscription: {ex.Message}");
                return false;
            }
        }

        private static async Task LoadUserSubscriptionData()
        {
            try
            {
                if (ActiveUserDto?.u_id == null) return;
                
                var subscription = await GetUserSubscriptionAsync(ActiveUserDto.u_id.Value);
                if (subscription != null)
                {
                    ActiveUserDto.u_subscription_plan = subscription.s_plan;
                    ActiveUserDto.u_is_subscribed = subscription.s_is_active;
                    ActiveUserDto.u_subscription_end = subscription.s_end_date;
                }
                else
                {
                    // No subscription found - set default values
                    ActiveUserDto.u_subscription_plan = "free";
                    ActiveUserDto.u_is_subscribed = false;
                    ActiveUserDto.u_subscription_end = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading subscription data: {ex.Message}");
                // Set default values on error
                if (ActiveUserDto != null)
                {
                    ActiveUserDto.u_subscription_plan = "free";
                    ActiveUserDto.u_is_subscribed = false;
                    ActiveUserDto.u_subscription_end = null;
                }
            }
        }

        public static void TriggerSubscriptionUpdated()
        {
            SubscriptionUpdated?.Invoke(null, EventArgs.Empty);
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