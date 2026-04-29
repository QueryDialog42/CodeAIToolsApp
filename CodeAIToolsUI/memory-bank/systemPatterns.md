# System Patterns

## Architecture Overview

### Layered Architecture
```
Presentation Layer (Views/UserControls)
    ↓
Business Logic Layer (RequestManager, Window Logic)
    ↓
Data Transfer Layer (DTOs)
    ↓
External Services (Backend API, GitHub API)
```

### Window Hierarchy
```
Window (Base)
├── StartWindow (Login/Register)
├── AdminWindow (Admin role)
└── WorkerWindow (Worker role)
    └── NewProjectWindow (Project creation)
```

## Key Design Patterns

### 1. Inheritance-Based Window Management
- **BaseAppWindow**: Abstract base class providing common functionality
  - Account popup management
  - File tree navigation
  - Project loading/saving
  - Theme application based on role
  - Abstract method `CreatePanelControl()` for role-specific panels

### 2. Centralized API Management
- **RequestManager**: Singleton-like static class for all API communication
  - Static HttpClient instance
  - Static ActiveUserDto for session management
  - Region-organized methods (Login, Register, Helpers)
  - Consistent error handling and response parsing

### 3. User Control Composition
- **AppMainControl**: Main editor interface with dual panes
  - Flow editor (diagrams)
  - Code editor (implementation)
  - Terminal output
  - Project cards
- **BasePanelControl**: Abstract base for role-specific panels
  - AdminPanelControl: Project/team management
  - WorkerPanelControl: Duty/task management

### 4. DTO Pattern
- **DTOs in APIs/DTOs/**: Data transfer objects for API communication
  - UserDto: User information
  - ProjectDto: Project metadata
  - DutyDto: Task assignments
  - AIRequestDto/AIResponseDto: AI service communication
  - AdminCollabDto: Admin collaboration data

### 5. Event-Driven UI Updates
- **FileTreeControl**: Custom tree view with events
  - OnFileSelected: File selection handling
  - ProjectChanged: Project switching
  - WriteFlowAndCodeLines: Loading file content into editors
- **PopupControl**: Account popup with events
  - CloseRequested: Popup dismissal
  - LogoutRequested: Session termination

## Component Relationships

### Authentication Flow
```
StartWindow
├── LoginControl → RequestManager.SendLoginRequest()
│   └── Success → AdminWindow/WorkerWindow (based on role)
└── RegisterControl → RequestManager.SendRegisterRequest()
    └── Success → Switch to LoginControl
```

### Project Management Flow
```
AdminWindow/WorkerWindow
├── BaseAppWindow.InitializeBase()
│   ├── LoadProjectsAsync() → FileTreeControl
│   └── ApplyRoleTheme() → Styles
├── AppMainControl (Project view)
│   ├── FileTreeControl (Navigation)
│   ├── Flow Editor (Flow.txt)
│   ├── Code Editor (Code.txt)
│   └── Terminal (Execution output)
└── BasePanelControl (Panel view)
    ├── AdminPanelControl (Admin only)
    └── WorkerPanelControl (Worker only)
```

### API Communication Pattern
```
UI Component
    ↓
RequestManager Static Method
    ↓
HttpClient.PostAsync/GetAsync
    ↓
JSON Serialization (Newtonsoft.Json)
    ↓
Backend API (localhost:8080)
    ↓
Response Handling (Status codes, DTOs)
    ↓
UI Update (MessageBox, Window navigation)
```

## Critical Implementation Paths

### 1. Application Startup
```
Program.Main()
    ↓
AppBuilder.Configure<App>()
    ↓
App.OnFrameworkInitializationCompleted()
    ↓
StartWindow displayed
```

### 2. User Login
```
LoginControl → RequestManager.SendLoginRequest()
    ↓
HTTP POST to /login and /getUser
    ↓
Set ActiveUserDto
    ↓
OpenRelatedWindow() (AdminWindow or WorkerWindow)
    ↓
Close StartWindow
```

### 3. Project Loading
```
BaseAppWindow.InitializeBase()
    ↓
CheckSystemRootFolderAsync()
    ↓
LoadProjectsAsync()
    ↓
FileTreeControl.LoadFiles() for each directory
    ↓
EnsureProjectStructureAsync() (Flow.txt, Code.txt)
```

### 4. Code Execution
```
AppMainControl.ExecuteButton_Clicked()
    ↓
ExecuteProcess()
    ↓
RunJavaCodeAsync() (Process.Start)
    ↓
Display output in Terminal
```

### 5. AI Integration
```
User triggers AI feature
    ↓
Create AIRequestDto
    ↓
HTTP POST to /AI/explain or /AI/transform
    ↓
Parse AIResponseDto
    ↓
Display result in UI
```

## State Management

### Global State
- **RequestManager.ActiveUserDto**: Current logged-in user (static)
- **BaseAppWindow.RootFolder**: System root directory path (static)

### Local State
- **Window-level**: Active project, editor content, panel visibility
- **Control-level**: File tree selection, terminal output, popup state
- **Session-level**: GitHub tokens, collaborator selections

### Persistence
- **Local Files**: Flow.txt, Code.txt in project directories
- **Backend API**: User data, projects, duties, teams
- **GitHub**: Repository data (via API)
