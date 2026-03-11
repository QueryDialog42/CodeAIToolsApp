using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using CodeAIToolsWPF;

namespace CodeAIToolsUI.UserControls
{
    public class FileTreeItem : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public int Depth { get; set; }
        public bool IsFolder { get; set; }
        public string FilePath { get; set; }
        public bool IsProjectRoot { get; set; } = false;
        public string Name => Path.GetFileName(FilePath);
        public ObservableCollection<FileTreeItem> Children { get; set; } = new();
        public string Icon => IsFolder ? (_isExpanded ? Icons.DIR_OPENED_ICON : Icons.DIR_CLOSED_ICON) : Icons.FILE_ICON;

        public SolidColorBrush FileForeground
        {
            get
            {
                if (IsProjectRoot)
                    return new SolidColorBrush(Color.Parse(Marks.ROOT_DIR_COLOR));

                return IsFolder
                    ? new SolidColorBrush(Color.Parse(Marks.DIR_COLOR))
                    : new SolidColorBrush(Colors.White);
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                OnPropertyChanged(nameof(Icon));
            }
        }

        public Thickness Indentation => new Thickness(Depth * 12, 3, 0, 3);

        public FileTreeItem(string filePath, bool isFolder = false, int depth = 0)
        {
            FilePath = filePath;
            IsFolder = isFolder;
            Depth = depth;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}