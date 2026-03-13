using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs.DTOs;

namespace CodeAIToolsUI.UserControls.MainControls
{
    public partial class ProjectCardControl : UserControl
    {
        private readonly ProjectDto _project;

        public event EventHandler<ProjectDto>? OpenRequested;
        public event EventHandler<ProjectDto>? DeleteRequested;

        public ProjectCardControl() : this(new ProjectDto()) { }
        
        public ProjectCardControl(ProjectDto project)
        {
            InitializeComponent();
            _project = project;
            Populate(project);
        }

        #region Project Methods

        private void Populate(ProjectDto project)
        {
            ProjectNameText.Text = project.p_name ?? "İsimsiz Proje";
            ProjectDescText.Text = project.p_description ?? "Açıklama yok.";
            ProjectDateText.Text = project.p_create_time.HasValue
                ? project.p_create_time.Value.ToString("dd.MM.yyyy")
                : "—";
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
            => OpenRequested?.Invoke(this, _project);

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
            => DeleteRequested?.Invoke(this, _project);
        
        #endregion
    }
}