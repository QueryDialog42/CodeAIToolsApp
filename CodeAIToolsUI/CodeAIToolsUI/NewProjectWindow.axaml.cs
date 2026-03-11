using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsWPF;
using MessageBox.Avalonia.Enums;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CodeAIToolsUI
{
    public partial class NewProjectWindow : Window
    {
        public NewProjectWindow()
        {
            InitializeComponent();
            // DragMove → BeginMoveDrag
            PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };
        }

        public string AramaMevcutu => TxtSearch.Text?.Trim() ?? "";
        public string ProjectName  => TxtProjectName.Text?.Trim() ?? "";
        public string Description  => TxtDescription.Text?.Trim() ?? "";

        #region Button Methods

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false); // DialogResult = false
        }

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Uyarı", "Lütfen bir proje adı girin.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                await box.ShowAsync();
                TxtProjectName.Focus();
                return;
            }

            CreateProject(ProjectName);
            Close(true); // DialogResult = true
        }

        #endregion

        #region Collaborator Methods

        private async void BtnAddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "İşbirlikçi Ekle", "İşbirlikçi ekleme açılacak.",
                ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowAsync();
        }

        private async void BtnRemoveCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "İşbirlikçi Kaldır", "İşbirlikçi kaldırma açılacak.",
                ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowAsync();
        }

        #endregion

        #region Project Text Methods

        private void TxtProjectName_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderName.IsVisible = string.IsNullOrEmpty(TxtProjectName.Text);

        private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderDesc.IsVisible = string.IsNullOrEmpty(TxtDescription.Text);

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => PlaceholderSearch.IsVisible = string.IsNullOrEmpty(TxtSearch.Text);

        #endregion

        #region Unclassified Methods

        private void CreateProject(string projectName)
        {
            string projectFolder = Path.Combine(MainWindow.RootFolder, projectName);
            string flowText = Path.Combine(projectFolder, Configs.FLOW_FILE);
            string codeText = Path.Combine(projectFolder, Configs.CODE_FILE);

            Directory.CreateDirectory(projectFolder);
            File.Create(flowText).Close();
            File.Create(codeText).Close();
        }

        #endregion
    }
}