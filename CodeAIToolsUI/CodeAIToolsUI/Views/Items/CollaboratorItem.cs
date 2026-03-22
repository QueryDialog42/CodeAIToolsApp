namespace CodeAIToolsUI.Views.Items;

public class CollaboratorItem
{
    public long     UserId   { get; set; }
    public string Username { get; set; } = "";
    public string GitEmail { get; set; } = "";
    public string Role     { get; set; } = "";
    public string Initial  => string.IsNullOrEmpty(Username) ? "?" 
        : Username[0].ToString().ToUpper();
}