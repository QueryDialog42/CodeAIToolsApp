using System;
using Avalonia;
using System.IO;
using MsBox.Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using System.Net.Http;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using MessageBox.Avalonia.Enums;
using CodeAIToolsUI.UserControls;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Styling;
using CodeAIToolsUI.UserControls.MainControls;
using CodeAIToolsUI.UserControls.PopupControl;
using CodeAIToolsUI.Services;
using Newtonsoft.Json;
using CodeAIToolsUI.APIs.DTOs;
using System.Collections.ObjectModel;
using System.Linq;



namespace CodeAIToolsUI.Views
{
    public abstract class BaseAppWindow : Window
    {
        public readonly HttpClient Http = new();

        private AppMainControl?    _mainControl;
        private BasePanelControl?  _panelControl;
        private ContentControl?    _mainContent;
        private TextBlock?         _accountLetter;
        private Popup?             _accountPopup;
        private BasePopupControl?  _popupContent;
        private FileTreeControl?   _fileTree;
        private TextBlock?         _activeProject;
        private Button?            _panelButton;
        private Button?            _projectButton;

        private ContentControl    MainContent    => _mainContent    ??= this.FindControl<ContentControl>("MainContent")!;
        private TextBlock         AccountLetter  => _accountLetter  ??= this.FindControl<TextBlock>("AccountLetter")!;
        private Popup             AccountPopup   => _accountPopup   ??= this.FindControl<Popup>("AccountPopup")!;
        public  BasePopupControl  PopupContent   => _popupContent   ??= this.FindControl<BasePopupControl>("AccountPopupContent")!;
        private FileTreeControl   FileTree       => _fileTree       ??= this.FindControl<FileTreeControl>("FileTree")!;
        private TextBlock         ActiveProject  => _activeProject  ??= this.FindControl<TextBlock>("ActiveProject")!;
        private Button            PanelButton    => _panelButton    ??= this.FindControl<Button>("PanelButton")!;
        private Button            ProjectButton  => _projectButton  ??= this.FindControl<Button>("ProjectButton")!;

        private string FlowText
        {
            get => MainControl.flowPage.editor.Text;
            set 
            { 
                MainControl.flowPage.editor.Text = value;
                ContentService.FlowContent = value;
            }
        }

        private string CodeText
        {
            get => MainControl.codePage.editor.Text;
            set 
            { 
                MainControl.codePage.editor.Text = value;
                ContentService.CodeContent = value;
            }
        }

        protected AppMainControl   MainControl   => _mainControl  ??= new AppMainControl();
        protected BasePanelControl PanelControl  => _panelControl ??= CreatePanelControl();

        protected abstract BasePanelControl CreatePanelControl();

        public static readonly string RootFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Configs.ROOT_DIR);

        private static readonly HashSet<string> SecretFolders = [Configs.LIBS_DIR];
        public ObservableCollection<string> _notifications = new();

        protected virtual void OnWindowInitialized() { }

        protected void InitializeBase()
        {
            SetupAccountArea();
            SetupFileTree();
            SetupPopupResize();
            _ = CheckSystemRootFolderAsync();
            ApplyRoleTheme();
            OnWindowInitialized();
        }

        private void ApplyRoleTheme()
        {
            bool isAdmin = RequestManager.ActiveUserDto?.u_role == Configs.ADMIN;
            var uri = isAdmin
                ? "avares://CodeAIToolsUI/Styles/AdminMainDict.axaml"
                : "avares://CodeAIToolsUI/Styles/WorkerMainDict.axaml";

            Application.Current!.Styles.Add(new StyleInclude(new Uri("avares://CodeAIToolsUI/App.axaml"))
            {
                Source = new Uri(uri)
            });
        }

        #region Button Methods

        protected void PanelButton_Clicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = PanelControl;
            FocusAndUnFocus(PanelButton, ProjectButton);
        }

        protected void ProjectButton_Clicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = MainControl;
            FocusAndUnFocus(ProjectButton, PanelButton);
        }

        protected void AccountButton_Click(object sender, PointerPressedEventArgs e)
        {
            var user = RequestManager.ActiveUserDto!;
            PopupContent.SetInfo(
                displayName:  user.u_name!,
                email:        user.u_email!,
                githubEmail:  user.u_git_email,
                isAdmin:      user.u_role == Configs.ADMIN
            );
            AccountPopup.IsOpen = !AccountPopup.IsOpen;
        }

        private static readonly SolidColorBrush FocusedColor   = new(Color.Parse("#FFFFFF"));
        private static readonly SolidColorBrush UnfocusedColor = new(Color.Parse("#888888"));

        private static void FocusAndUnFocus(Button focused, Button unfocused)
        {
            focused.Foreground   = FocusedColor;
            focused.FontWeight   = FontWeight.SemiBold;
            unfocused.Foreground = UnfocusedColor;
            unfocused.FontWeight = FontWeight.Normal;
        }

        #endregion

        #region Context Menu Methods

        protected void File_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { ContextMenu: { } menu } button)
            {
                menu.Placement = PlacementMode.Bottom;
                menu.Open(button);
            }
        }

        protected async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveProject.Text is not { } projectName) return;

            var projectPath = Path.Combine(RootFolder, projectName);

            try
            {
                await File.WriteAllTextAsync(Path.Combine(projectPath, Configs.FLOW_FILE), FlowText.Trim());
                await File.WriteAllTextAsync(Path.Combine(projectPath, Configs.CODE_FILE), CodeText.Trim());
            }
            catch (IOException)
            {
                await ShowMessage("Saving Failed", "Something went wrong while writing.");
            }
        }

        protected void Edit_Click(object sender, RoutedEventArgs e) { }

        #endregion

        #region Helpers

        public async Task LoadProjectsAsync()
        {
            FileTree.Clear();
            foreach (var dir in Directory.GetDirectories(RootFolder))
            {
                if (SecretFolders.Contains(Path.GetFileName(dir))) continue;
                await EnsureProjectStructureAsync(dir);
                FileTree.LoadFiles(dir);
            }
        }

        private async Task EnsureProjectStructureAsync(string projectPath)
        {
            await EnsureFileExistsAsync(Path.Combine(projectPath, Configs.FLOW_FILE));
            await EnsureFileExistsAsync(Path.Combine(projectPath, Configs.CODE_FILE));
        }

        private async Task EnsureFileExistsAsync(string filePath)
        {
            if (File.Exists(filePath)) return;

            await ShowMessage("File Not Found", $"{Path.GetFileName(filePath)} could not be found. Creating.");
            File.Create(filePath).Close();
        }

        private async Task CheckSystemRootFolderAsync()
        {
            if (!Directory.Exists(RootFolder))
            {
                await ShowMessage("Root Folder Not Found", "System root folder could not be found. Creating.");
                Directory.CreateDirectory(RootFolder);
            }
            await LoadProjectsAsync();
        }

        private static string ReadLines(string[]? lines) =>
            lines is null ? "" : string.Join(Environment.NewLine, lines) + Environment.NewLine;

        private static async Task ShowMessage(string title, string message) =>
            await MessageBoxManager
                .GetMessageBoxStandard(title, message, ButtonEnum.Ok,  MsBox.Avalonia.Enums.Icon.Info)
                .ShowAsync();

        private void SetupAccountArea()
        {
            // Only setup account area if the controls exist
            if (this.FindControl<TextBlock>("AccountLetter") != null)
            {
                AccountLetter.Text = GetInitial(RequestManager.ActiveUserDto?.u_email);
            }
            
            if (this.FindControl<ContentControl>("MainContent") != null && MainControl != null)
            {
                MainContent.Content = MainControl;
            }

            if (this.FindControl<BasePopupControl>("AccountPopupContent") != null)
            {
                PopupContent.CloseRequested += (_, _) => AccountPopup?.IsOpen = false;
                PopupContent.LogoutRequested += (_, _) => { new StartWindow().Show(); Close(); };
            }
        }

        private void SetupFileTree()
        {
            // Only setup file tree if the controls exist
            if (this.FindControl<FileTreeControl>("FileTree") != null)
            {
                FileTree.OnFileSelected += async (item) => await ShowMessage("Bilgi", $"Seçilen dosya: {item.FilePath}");
                
                if (this.FindControl<TextBlock>("ActiveProject") != null)
                {
                    FileTree.ProjectChanged += (item) => ActiveProject.Text = Path.GetFileName(item.FilePath);
                }
                
                FileTree.WriteFlowAndCodeLines += (flow, code) => { FlowText = ReadLines(flow); CodeText = ReadLines(code); };
                FileTree.WriteDenieMessages += (flowDenie, codeDenie) =>
                {
                    MainControl.flowCommentTextBox.Text = flowDenie;
                    MainControl.codeCommentTextBox.Text = codeDenie;
                };
            }
        }

        private void SetupPopupResize()
        {
            // Only setup popup resize if the controls exist
            if (this.FindControl<BasePopupControl>("AccountPopupContent") != null && 
                this.FindControl<Popup>("AccountPopup") != null)
            {
                PopupContent.SizeChanged += (_, _) =>
                {
                    AccountPopup.HorizontalOffset += 1;
                    AccountPopup.HorizontalOffset -= 1;
                };
            }
        }

        protected virtual async Task LoadAiSelectorCore(ComboBox aiSelector)
        {
            // Get user's API settings first
            var baseUrl = string.Empty;
            var apiKey = string.Empty;

            var settingsResponse = await Http.GetAsync(ApiEndpoints.GET_BAS_URL_AND_KEY_API + RequestManager.ActiveUserDto?.u_id);
            if (settingsResponse.IsSuccessStatusCode)
            {
                var content = await settingsResponse.Content.ReadAsStringAsync();
                var baseUrlAndKey = JsonConvert.DeserializeObject<List<string>>(content);

                baseUrl = baseUrlAndKey[0];
                apiKey = baseUrlAndKey[1];
            }

            if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(apiKey))
            {
                var modelsUrl = $"{ApiEndpoints.GET_AI_MODS_API}/{RequestManager.ActiveUserDto?.u_id}";
                
                var response = await Http.GetAsync(modelsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var models = JsonConvert.DeserializeObject<List<string>>(content);
                    aiSelector.ItemsSource = models;
                }
            }
        }

        public async Task OpenSettingsDialog(){
            var baseUrl = string.Empty;
            var apikey = string.Empty;
            var sendUrl = string.Empty;

            var response = await Http.GetAsync(ApiEndpoints.GET_BAS_URL_AND_KEY_API + RequestManager.ActiveUserDto?.u_id);
            if (response.IsSuccessStatusCode){
                var content = await response.Content.ReadAsStringAsync();
                var baseUrlAndKey = JsonConvert.DeserializeObject<List<string>>(content);
                baseUrl = baseUrlAndKey[0];
                apikey = baseUrlAndKey[1];
                sendUrl = baseUrlAndKey[2];
            }
        
           var dialog = new SettingsWindow(
            currentBaseUrl: baseUrl,
            currentApiKey: apikey,
            currentSendUrl: sendUrl
           );
            
            await dialog.ShowDialog<bool>(this);
        }

        private static string GetInitial(string? email) =>
            email?.Length > 0 ? email[0].ToString().ToUpper() : "U";

        #endregion

        public async Task<ObservableCollection<string>> GetNotifications(List<int> adminIds)
        {
            var allNotifications = new List<string>();

            foreach (var adminId in adminIds)
            {
                var response = await Http.GetAsync(ApiEndpoints.GET_NOTS_API + "/" + adminId);
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                var notifications = JsonConvert.DeserializeObject<List<NotificationDto>>(content);

                if (notifications == null) continue;

                // Her admin'in bildirimlerini listeye ekle
                allNotifications.AddRange(
                    notifications.Select(n => $"📋 {n.project_name} is denied by your admin")
                );
            }

            // Mevcut _notifications'a ekle (replace değil)
            foreach (var item in allNotifications)
                _notifications.Add(item);

            return _notifications;
        }
    }
}