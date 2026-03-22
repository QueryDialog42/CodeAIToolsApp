using CodeAIToolsUI.UserControls.MainControls;

namespace CodeAIToolsUI.Views
{
    public partial class AdminWindow : BaseAppWindow
    {
        protected override BasePanelControl CreatePanelControl() => new AdminPanelControl();
        public AdminWindow()
        {
            InitializeComponent();
            InitializeBase();
        }
    }
}