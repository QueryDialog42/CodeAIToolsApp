using System;
using Avalonia;
using System.IO;
using System.Linq;
using MsBox.Avalonia;
using Avalonia.Controls;
using CodeAIToolsUI.Views;
using MsBox.Avalonia.Enums;
using Avalonia.Interactivity;
using MessageBox.Avalonia.Enums;
using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;



namespace CodeAIToolsUI.UserControls
{
    public partial class FileTreeControl : UserControl
    {
        public event Action<FileTreeItem>? OnFileSelected;
        public event Action<FileTreeItem>? ProjectChanged;
        public event Action<string[], string[]>? WriteFlowAndCodeLines;
        private ObservableCollection<FileTreeItem> AllItems { get; set; } = new();

        public FileTreeControl()
        {
            InitializeComponent();
            FileTreeItems.ItemsSource = AllItems;
        }

        #region Item Management Methods

        public void AddItem(FileTreeItem item) => AllItems.Add(item);

        private void FileItem_Clicked(object sender, RoutedEventArgs e)
        {
            var item = (FileTreeItem)((Button)sender).Tag!;

            if (item.IsFolder)
            {
                if (item.IsProjectRoot)
                {
                    ProjectChanged?.Invoke(item);
                    ReadFlowAndCodeFiles(item.FilePath);
                }

                if (item.IsExpanded)
                    CollapseItem(item);
                else
                    ExpandItem(item);

                item.IsExpanded = !item.IsExpanded;
            }
            else
            {
                OnFileSelected?.Invoke(item);
            }
        }

        private void ExpandItem(FileTreeItem folder)
        {
            int index = AllItems.IndexOf(folder);
            foreach (var child in folder.Children ?? [])
                AllItems.Insert(++index, child);
        }

        private void CollapseItem(FileTreeItem folder)
        {
            foreach (var child in folder.Children ?? [])
            {
                if (child is { IsFolder: true, IsExpanded: true })
                {
                    CollapseItem(child);
                    child.IsExpanded = false;
                }

                AllItems.Remove(child);
            }
        }

        #endregion

        #region File Operations Methods

        private void ReadFlowAndCodeFiles(string rootPath)
        {
            string[] flowLines = [];
            string[] codeLines = [];

            try
            {
                var flowPath = Path.Combine(rootPath, Configs.FLOW_FILE);
                if (File.Exists(flowPath))
                    flowLines = File.ReadAllLines(flowPath);
            }
            catch (Exception ex)
            {
                ShowMessageAsync("Hata", $"Flow dosyası okunamadı: {ex.Message}");
            }

            try
            {
                var codePath = Path.Combine(rootPath, Configs.CODE_FILE);
                if (File.Exists(codePath))
                    codeLines = File.ReadAllLines(codePath);
            }
            catch (Exception ex)
            {
                ShowMessageAsync("Hata", $"Code dosyası okunamadı: {ex.Message}");
            }

            WriteFlowAndCodeLines?.Invoke(flowLines, codeLines);
        }

        private async void DeleteProject_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Avalonia'da ContextMenu.PlacementTarget → Tag üzerinden erişim
                if (sender is not MenuItem menuItem) return;
                if (menuItem.Parent is not ContextMenu contextMenu) return;
                if (contextMenu.Tag is not TextBlock textBlock) return;

                var item = textBlock.Tag as FileTreeItem;
                var name = item?.Name;

                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Proje Sil",
                    $"'{name}' projesini silmek istediğinize emin misiniz?",
                    ButtonEnum.YesNo, Icon.Warning);

                var result = await box.ShowAsync();

                if (result == ButtonResult.Yes && item != null)
                {
                    Directory.Delete(item.FilePath, true);
                }

                Clear();

                // Application.Current.Windows → ApplicationLifetime
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Windows.OfType<AdminWindow>().FirstOrDefault()?.LoadProjectsAsync();
                }
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException($"An error occured while deleting the project, {ex}");
            }
        }

        public void LoadFiles(string folderPath, FileTreeItem? parent = null, int depth = 0)
        {
            if (parent == null)
            {
                var projectRoot = new FileTreeItem(folderPath, isFolder: true, depth: 0)
                {
                    IsProjectRoot = true
                };
                LoadFiles(folderPath, projectRoot, depth + 1);
                AllItems.Add(projectRoot);

                CollapseItem(projectRoot);
                projectRoot.IsExpanded = false;
                return;
            }

            var collection = parent.Children;

            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                var folder = new FileTreeItem(dir, true, depth);
                LoadFiles(dir, folder, depth + 1);
                collection?.Add(folder);
            }

            foreach (var file in Directory.GetFiles(folderPath))
                collection?.Add(new FileTreeItem(file, depth: depth));
        }

        #endregion

        
        #region Unclassified Methods

        public void Clear() => AllItems.Clear();

        // Fire-and-forget mesaj gösterici (event handler dışında kullanım için)
        private async void ShowMessageAsync(string title, string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Info);
            await box.ShowAsync();
        }

        #endregion
    }
}