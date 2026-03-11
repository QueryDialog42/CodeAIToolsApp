using Avalonia.Controls;
using CodeAIToolsUI.UserControls.StartControls;

namespace CodeAIToolsUI;

public partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();
        startContent.Content = new LoginControl();
    }
}