using System;
using Avalonia.Media;
using Avalonia.Controls;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;

namespace CodeAIToolsUI.UserControls.PopupControl;
public partial class CollaboratorItemControl : UserControl
{
    public event EventHandler? RemoveRequested;

    public CollaboratorItemControl(UserDto userDto, bool isAdmin)
    {
        InitializeComponent();
        SetAccountStyles(userDto, isAdmin);
        DecideRemoveButton();
    }

    private void SetAccountStyles(UserDto? userDto, bool isAdmin)
    {
        if (userDto == null) return;
        
        var colorBrush = isAdmin ? "#1e3a8a" : "#064e3b"; 
        
        UserNameText.Text = userDto.u_name;
        AvatarText.Text   = userDto.u_name?.Length > 0
            ? userDto.u_name[0].ToString().ToUpper()
            : "?";
        Avatar.Background = new SolidColorBrush(Color.Parse(colorBrush));
    }

    private void DecideRemoveButton()
    {
        if (RequestManager.ActiveUserDto!.u_role!.ToUpper() == Configs.ADMIN)
            RemoveBtn.Click     += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        else
            RemoveBtn.IsVisible = false;            
    }
}