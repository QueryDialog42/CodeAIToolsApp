using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using CodeAIToolsUI.UserControls.MainControls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MessageBox.Avalonia.Enums;
using Newtonsoft.Json;

namespace CodeAIToolsUI.Views
{
    public partial class SubscriptionWindow : BaseAppWindow
    {
        private readonly HttpClient _http = new();

        public SubscriptionWindow()
        {
            InitializeComponent();
            InitializeBase();
        }

        protected override BasePanelControl CreatePanelControl()
        {
            // This window doesn't need a panel control
            return null!;
        }

        private async void PremiumButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateSubscription("premium");
        }

        private async void ProButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateSubscription("pro");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task CreateSubscription(string plan)
        {
            try
            {
                var user = RequestManager.ActiveUserDto;
                if (user?.u_id == null)
                {
                    await ShowError("User not found. Please log in again.");
                    return;
                }

                var subscription = new CreateSubscriptionDto
                {
                    u_id = user.u_id.Value,
                    s_plan = plan,
                    payment_method = "credit_card", // You'll implement payment processing
                    payment_token = "demo_token" // Replace with actual payment token
                };

                var json = JsonConvert.SerializeObject(subscription);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(ApiEndpoints.CREATE_SUBSCRIPTION_API, content);

                if (response.IsSuccessStatusCode)
                {
                    await ShowSuccess($"Successfully subscribed to {plan} plan!");
                    
                    // Update user subscription info
                    await UpdateUserSubscriptionInfo(plan);
                    
                    Close();
                }
                else
                {
                    await ShowError($"Failed to create subscription: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error creating subscription: {ex.Message}");
            }
        }

        private async Task UpdateUserSubscriptionInfo(string plan)
        {
            try
            {
                var response = await _http.GetAsync($"{ApiEndpoints.GET_SUBSCRIPTION_API}?user_id={RequestManager.ActiveUserDto?.u_id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var subscription = JsonConvert.DeserializeObject<SubscriptionDto>(responseBody);
                    
                    // Update the active user info
                    if (RequestManager.ActiveUserDto != null)
                    {
                        RequestManager.ActiveUserDto.u_subscription_plan = plan;
                        RequestManager.ActiveUserDto.u_is_subscribed = true;
                        RequestManager.ActiveUserDto.u_subscription_end = subscription?.s_end_date;
                        
                        // Trigger language selector refresh event
                        RequestManager.TriggerSubscriptionUpdated();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as subscription was created
                System.Diagnostics.Debug.WriteLine($"Error updating subscription info: {ex.Message}");
            }
        }

        private async Task ShowSuccess(string message)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Success", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success)
                .ShowAsync();
        }

        private async Task ShowError(string message)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Error", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
        }
    }
}
