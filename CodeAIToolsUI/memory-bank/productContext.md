# Product Context

## Why This Project Exists
CodeAIToolsUI addresses the need for a collaborative development environment that combines traditional coding workflows with modern AI assistance. It bridges the gap between individual coding tasks and team-based project management while providing intelligent code support.

## Problems It Solves
1. **Fragmented Development Tools**: Combines code editing, flow diagramming, and AI assistance in a single interface
2. **Team Coordination**: Provides role-based access control for admins and workers in collaborative projects
3. **AI Integration Barriers**: Makes AI code explanation and transformation accessible directly within the development environment
4. **Local-Remote Sync**: Maintains local project files while syncing with backend services for team collaboration
5. **GitHub Management**: Simplifies repository creation and management for team projects

## How It Should Work
1. **Authentication Flow**: Users register/login via backend API, receive role assignment (ADMIN/WORKER)
2. **Window Navigation**: Based on role, users see either AdminWindow or WorkerWindow with appropriate features
3. **Project Creation**: Admins create projects, select collaborators, and optionally create GitHub repositories
4. **Code Development**: Users select projects from file tree, edit Flow.txt (diagrams) and Code.txt (implementation)
5. **AI Assistance**: Users can request AI explanations or code transformations through backend API
6. **Team Management**: Admins assign duties to workers, manage team members
7. **Local Storage**: Projects stored in `~/CodeAI_Root/` with standardized structure (Flow.txt, Code.txt)

## User Experience Goals
- **Role-Specific Interfaces**: Clear distinction between admin and worker capabilities
- **Intuitive Navigation**: Easy switching between projects, panels, and editor views
- **Real-time Feedback**: Terminal output for code execution, AI responses displayed promptly
- **Visual Clarity**: Color-coded themes (blue for admin, green for worker) for role identification
- **Efficient Workflows**: Quick access to common actions (save, execute, AI features)

## Technical Philosophy
- **Desktop-First**: Native desktop application using Avalonia for cross-platform support
- **API-Driven**: All data persistence and AI services handled by backend API
- **Local-First**: Projects stored locally with API sync for collaboration
- **Role-Based Security**: All operations validated against user role on backend
