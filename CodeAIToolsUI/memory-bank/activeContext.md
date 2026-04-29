# Active Context

## Current Work Focus
This memory bank is being created to document the CodeAIToolsUI project for future development work. The project is a desktop application built with Avalonia UI that provides a collaborative code development environment with AI integration.

## Recent Changes
- Memory bank structure created following AGENT.md guidelines
- Project analysis completed to understand architecture and components
- Documentation files created: projectbrief.md, productContext.md, techContext.md, systemPatterns.md

## Next Steps
- Complete progress.md documentation
- Document any additional context needed for specific features
- Consider creating feature-specific documentation for complex areas (AI integration, GitHub management, etc.)

## Active Decisions and Considerations

### Memory Bank Structure
- Following the structure defined in AGENT.md for consistency
- Core files created: projectbrief, productContext, techContext, systemPatterns
- Additional files can be added as needed for complex features

### Project Understanding
- **Architecture**: Layered architecture with BaseAppWindow as foundation
- **Roles**: Two distinct user roles (ADMIN/WORKER) with different windows
- **API Integration**: Heavy reliance on backend API at localhost:8080
- **Local Storage**: Projects stored in ~/CodeAI_Root/ with Flow.txt and Code.txt

### Key Patterns Identified
- Inheritance-based window management via BaseAppWindow
- Centralized API communication through RequestManager
- Event-driven UI updates for file tree and popups
- DTO pattern for API data transfer

## Important Patterns and Preferences

### Code Organization
- UserControls organized by function (MainControls, PopupControl, StartControls, TeamControl)
- DTOs separated in APIs/DTOs/ directory
- Styles in dedicated Styles/ directory with role-specific themes

### Naming Conventions
- DTOs end with "Dto" suffix
- UserControls end with "Control" suffix
- Windows end with "Window" suffix
- Constants organized in Configs, Marks, Icons, ApiEndpoints structs

### UI Patterns
- XAML for UI definition with code-behind (.axaml.cs)
- Compiled bindings enabled by default
- Resource dictionaries for theming
- Emoji icons used throughout (defined in Icons struct)

### Error Handling
- GeneralRoutines class for common error patterns
- MessageBox dialogs for user feedback
- Try-catch blocks with user-friendly messages
- Status code handling in API responses

## Learnings and Project Insights

### Architecture Strengths
- Clean separation between presentation and business logic
- Reusable base classes reduce code duplication
- Centralized API management simplifies backend communication
- Role-based theming provides clear visual distinction

### Potential Areas for Improvement
- Heavy dependency on backend API (no offline mode)
- Beta/RC versions of some dependencies (Newtonsoft.Json, MsBox.Avalonia)
- Windows-specific features (COM interop) limit cross-platform potential
- Large code-behind files (AppMainControl.axaml.cs is 693 lines)

### Development Considerations
- Backend API must be running for full functionality
- Local file system permissions required for project storage
- GitHub token management needed for repository features
- Java execution requires Java runtime environment

### Testing Considerations
- API integration testing requires mock backend
- File system operations need proper cleanup in tests
- UI testing with Avalonia requires specific test setup
- Role-based features need testing for both ADMIN and WORKER roles
