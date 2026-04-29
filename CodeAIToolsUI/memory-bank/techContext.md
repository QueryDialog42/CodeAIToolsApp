# Tech Context

## Technology Stack

### Core Framework
- **Avalonia UI 11.3.12**: Cross-platform XAML-based UI framework
- **.NET 10.0**: Target framework for the application
- **C#**: Primary programming language

### UI Components
- **Avalonia.AvaloniaEdit 11.4.1**: Code editor component with syntax highlighting
- **Avalonia.Themes.Fluent 11.3.12**: Fluent design theme
- **Avalonia.Fonts.Inter 11.3.12**: Inter font family

### Third-Party Libraries
- **LibGit2Sharp 0.31.0**: Git operations for GitHub integration
- **Octokit 14.0.0**: GitHub API client
- **Newtonsoft.Json 13.0.5-beta1**: JSON serialization
- **MsBox.Avalonia 3.0.0-rc2**: Message box dialogs

### Development Tools
- **Visual Studio**: Primary IDE (based on .sln file)
- **Avalonia Diagnostics 11.3.12**: Debug tools for development

## Development Setup

### Project Structure
```
CodeAIToolsUI/
├── CodeAIToolsUI/           # Main project directory
│   ├── APIs/                # API communication layer
│   │   ├── DTOs/           # Data transfer objects
│   │   └── RequestManager.cs
│   ├── UserControls/        # Reusable UI components
│   │   ├── MainControls/   # Primary application controls
│   │   ├── PopupControl/   # Popup dialogs
│   │   ├── StartControls/  # Login/register controls
│   │   └── TeamControl/    # Team management
│   ├── Views/               # Window definitions
│   ├── Styles/              # XAML style resources
│   ├── CustomTemplates/    # Custom templates
│   └── App.axaml           # Application entry point
├── CodeAIToolsUI.sln       # Solution file
└── AGENT.md                # Memory bank documentation
```

### Build Configuration
- **Output Type**: WinExe (Windows executable)
- **Build Platforms**: Debug|Any CPU, Release|Any CPU
- **Application Manifest**: app.manifest (for Windows compatibility)
- **COM Interop**: Enabled for Windows integration

### Key Dependencies
- Backend API running on `http://localhost:8080`
- GitHub API for repository management
- Local file system for project storage

## Technical Constraints

### Backend Dependency
- Application requires backend API to be running on localhost:8080
- All user authentication, project data, and AI services depend on backend
- No offline mode available for core features

### Platform Support
- Designed for Windows (WinExe output type)
- Avalonia provides cross-platform potential but currently Windows-focused
- COM interop enabled suggests Windows-specific features

### File System
- Projects stored in user profile: `~/CodeAI_Root/`
- Requires write permissions to user profile directory
- Standardized project structure: Flow.txt and Code.txt required

### API Versioning
- Using beta version of Newtonsoft.Json (13.0.5-beta1)
- MsBox.Avalonia in release candidate (3.0.0-rc2)
- May need updates for production stability

## Tool Usage Patterns

### XAML/Avalonia Patterns
- Compiled bindings enabled by default
- Resource dictionaries for styling (Styles/ directory)
- UserControls for reusable components
- Window inheritance via BaseAppWindow

### Code Editor Integration
- AvaloniaEdit for code editing with syntax highlighting
- Custom Java syntax highlighting (Java.xshd)
- Dual editor layout (flow + code)

### API Communication
- HttpClient for all backend communication
- JSON serialization with Newtonsoft.Json
- Async/await pattern for all API calls
- RequestManager as centralized API handler

### Error Handling
- MessageBox dialogs for user feedback
- Try-catch blocks with user-friendly messages
- GeneralRoutines for common error patterns
