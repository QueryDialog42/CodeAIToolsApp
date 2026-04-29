using System;
using Avalonia.Media;
using System.Net.Http;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using Avalonia.Interactivity;
using System.Threading.Tasks;


namespace CodeAIToolsUI.UserControls.PopupControl
{
    public partial class BasePopupControl : UserControl
    {
        
        // ── Tek HttpClient instance — socket exhaustion önlenir ───────────────
        protected static readonly HttpClient Http = new();
        
        readonly Window _window = new Window
        {
            //Content                  = searchControl,
            SizeToContent            = SizeToContent.WidthAndHeight,
            WindowStartupLocation    = WindowStartupLocation.CenterScreen,
            SystemDecorations        = SystemDecorations.None,
            TransparencyLevelHint    = new[] { WindowTransparencyLevel.Transparent },
            Background               = Brushes.Transparent,
            CanResize                = false,
            ShowInTaskbar            = false,
            Topmost                  = true
        };
        
        public event EventHandler? CloseRequested;
        public event EventHandler? LogoutRequested;
        
        protected BasePopupControl()
        {
            InitializeComponent();
        }
        
        
        #region Button Event Handlers

        protected void Close_Click(object sender, RoutedEventArgs e)
            => CloseRequested?.Invoke(this, EventArgs.Empty);

        protected void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        protected void Logout_Click(object sender, RoutedEventArgs e)
        {
            RequestManager.ActiveUserDto = null;
            LogoutRequested?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Shared Methods
        
        

        public void SetInfo(string displayName, string email,
                            string? githubEmail = null, bool isAdmin = false)
        {
            AvatarInitials.Text = displayName.Length > 0
                ? displayName[0].ToString().ToUpper() : "?";
            DisplayName.Text = displayName;
            EmailText.Text = email;

            if (!string.IsNullOrWhiteSpace(githubEmail))
            {
                GithubEmailText.Text = githubEmail;
                GithubCard.IsVisible = true;
            }
            else
            {
                GithubCard.IsVisible = false;
            }

            if (isAdmin)
            {
                RoleLabel.Text = char.ToUpper(Configs.ADMIN[0]) + Configs.ADMIN[1..].ToLower();
                RoleIcon.Text = Icons.ADM_ICON;
                RoleBadge.Background = new SolidColorBrush(Color.Parse(Marks.ADM_POP_TITLE_COL));
            }
            else
            {
                RoleLabel.Text = char.ToUpper(Configs.WORKER[0]) + Configs.WORKER[1..].ToLower();
                RoleIcon.Text = Icons.WORK_ICON;
                RoleBadge.Background = new SolidColorBrush(Color.Parse(Marks.WORK_POP_TITLE_COL));
            }

            // Set subscription information from active user
            SetSubscriptionInfo();
        }

        private void SetSubscriptionInfo()
        {
            var user = RequestManager.ActiveUserDto;
            if (user != null)
            {
                // Display subscription plan
                string plan = user.u_subscription_plan ?? "free";
                SubscriptionPlanText.Text = plan.ToUpper() == "FREE" ? "Free" : 
                                          plan.ToUpper() == "PREMIUM" ? "Premium" : 
                                          plan.ToUpper() == "PRO" ? "Pro" : plan;

                // Display subscription status
                bool isActive = user.u_is_subscribed == true && 
                               user.u_subscription_end.HasValue && 
                               user.u_subscription_end.Value > DateTime.Now;

                if (isActive)
                {
                    SubscriptionBadgeText.Text = "AKTİF";
                    SubscriptionBadge.Background = new SolidColorBrush(Color.Parse("#22c55e")); // Green
                }
                else
                {
                    SubscriptionBadgeText.Text = plan.ToUpper() == "FREE" ? "FREE" : "EXPIRED";
                    SubscriptionBadge.Background = new SolidColorBrush(Color.Parse("#ef4444")); // Red
                }
            }
            else
            {
                SubscriptionPlanText.Text = "Bilinmiyor";
                SubscriptionBadgeText.Text = "—";
                SubscriptionBadge.Background = new SolidColorBrush(Color.Parse("#6b7280")); // Gray
            }
        }
        
        protected static string BuildUrl(string baseUrl, object id)
            => $"{baseUrl.TrimEnd('/')}/{id}";
        
        private async void AddCollaborator_Click(object sender, RoutedEventArgs e) => await OnCollaboratorAddClick(_window);

        protected virtual Task OnCollaboratorAddClick(Window window) => Task.CompletedTask;

        protected virtual void OnLoaded(object sender, RoutedEventArgs e){}

        #endregion
    }
}