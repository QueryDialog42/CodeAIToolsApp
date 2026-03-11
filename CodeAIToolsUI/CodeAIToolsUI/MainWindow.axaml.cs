using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.UserControls.MainControls;
using MsBox.Avalonia;
using CodeAIToolsWPF;
using MessageBox.Avalonia.Enums;

namespace CodeAIToolsUI
{
    public partial class MainWindow : Window
    {
        AdminMainControl _adminMainControl = new();
        AdminPanelControl _adminPanelControl = new();

        public static string RootFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Configs.ROOT_DIR);

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
            CheckSystemRootFolder();
        }

        #region Button Methods

        private void PanelButton_Clicked(object sender, RoutedEventArgs e)
        {
            mainContent.Content = _adminPanelControl;
            FocusAndUnFocus(panelButton, projectButton);
        }

        private void ProjectButton_Clicked(object sender, RoutedEventArgs e)
        {
            mainContent.Content = _adminMainControl;
            FocusAndUnFocus(projectButton, panelButton);
        }

        // PointerPressed event handler — XAML'da PointerPressed="AccountButton_Click" olmalı
        private void AccountButton_Click(object sender, PointerPressedEventArgs e)
        {
            accountPopupContent.SetInfo(
                displayName: "Ad soyad",
                email: RequestManager.activeUserDto?.u_email ?? "null",
                githubEmail: RequestManager.activeUserDto?.u_git_email,
                isAdmin: RequestManager.activeUserDto?.u_role == Configs.ADMIN
            );
            accountPopup.IsOpen = !accountPopup.IsOpen;
        }

        private void FocusAndUnFocus(Button buttonToFocus, Button buttonToUnfocus)
        {
            buttonToFocus.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            buttonToFocus.FontWeight = FontWeight.SemiBold;

            buttonToUnfocus.Foreground = new SolidColorBrush(Color.Parse("#888888"));
            buttonToUnfocus.FontWeight = FontWeight.Normal;
        }

        #endregion

        #region Initialize Methods

        public void LoadProjects()
        {
            fileTree.Clear();

            foreach (var dir in Directory.GetDirectories(RootFolder))
            {
                string[] secretFolders = { Configs.LIBS_DIR };
                if (secretFolders.Contains(Path.GetFileName(dir))) continue;

                CheckProjectStructure(dir);
                fileTree.LoadFiles(dir);
            }
        }

        private void InitializeApplication()
        {
            string activeEmail = RequestManager.activeUserDto?.u_email ?? "";
            accountLetter.Text = activeEmail.Length > 0
                ? activeEmail[0].ToString().ToUpper()
                : "U";

            accountPopupContent.CloseRequested += (_, _) => accountPopup.IsOpen = false;

            mainContent.Content = _adminMainControl;

            accountPopupContent.SizeChanged += (_, _) =>
            {
                accountPopup.HorizontalOffset += 1;
                accountPopup.HorizontalOffset -= 1;
            };

            fileTree.OnFileSelected += async (item) =>
            {
                await ShowMessage("Bilgi", $"Seçilen dosya: {item.FilePath}");
            };

            fileTree.ProjectChanged += (item) =>
            {
                var projectName = Path.GetFileName(item.FilePath);
                activeProject.Text = projectName;
            };

            fileTree.WriteFlowAndCodeLines += (flowLines, codeLines) =>
            {
                var flowText = ReadLines(flowLines);
                var codeText = ReadLines(codeLines);
                _adminMainControl.flowPage.editor.Text = flowText;
                _adminMainControl.codePage.editor.Text = codeText;
            };

            accountPopupContent.LogoutRequested += (_, _) =>
            {
                new StartWindow().Show();
                Close();
            };
        }

        private async void CheckSystemRootFolder()
        {
            if (!Directory.Exists(RootFolder))
            {
                await ShowMessage("Root Folder Not Found",
                    "System root folder could not be found. Creating.");
                Directory.CreateDirectory(RootFolder);
            }
            LoadProjects();
        }

        private async void CheckIfFileExist(string filePath)
        {
            if (!File.Exists(filePath))
            {
                await ShowMessage("Project File Not Found",
                    $"{Path.GetFileName(filePath)} could not be found. Creating.");
                File.Create(filePath).Close();
            }
        }

        public void CheckProjectStructure(string projectPath)
        {
            string flowFile = Path.Combine(projectPath, Configs.FLOW_FILE);
            string codeFile = Path.Combine(projectPath, Configs.CODE_FILE);

            CheckIfFileExist(flowFile);
            CheckIfFileExist(codeFile);
        }

        #endregion

        #region Context Menu Methods

        private void File_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                // Avalonia'da PlacementTarget ve Placement doğrudan ContextMenu üzerinde set edilir
                button.ContextMenu.Placement = PlacementMode.Bottom;
                button.ContextMenu.Open(button);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (activeProject?.Text == null) return;

            var flowText = _adminMainControl.flowPage.editor.Document.Text.Trim();
            var codeText = _adminMainControl.codePage.editor.Document.Text.Trim();

            var projectPath = Path.Combine(RootFolder, activeProject.Text);
            try
            {
                await File.WriteAllTextAsync(Path.Combine(projectPath, Configs.FLOW_FILE), flowText);
                await File.WriteAllTextAsync(Path.Combine(projectPath, Configs.CODE_FILE), codeText);
            }
            catch (IOException)
            {
                await ShowMessage("Saving Failed",
                    "Something went wrong while writing. Please try again later.");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Düzenle işlemi
        }

        #endregion

        #region Unclassified Methods

        private string ReadLines(string[]? lines)
        {
            if (lines == null) return "";
            var text = new StringBuilder();
            foreach (var line in lines)
                text.AppendLine(line);
            return text.ToString();
        }

        private static async Task ShowMessage(string title, string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                title, message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowAsync();
        }

        #endregion
    }
}