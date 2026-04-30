# CodeAI Tools Active Context

## Current Work Focus
The project is in active development with a complete backend database service and desktop frontend application. The system is functional with user authentication, project management, and AI integration features implemented.

## Recent Changes
- Spring Boot backend with JPA entities for Users, Projects, Teams, Duties, GitTokens, and Subscriptions
- Avalonia UI desktop application with role-based windows (Admin/Worker)
- RESTful API endpoints for user management, project operations, and AI services
- GitHub integration for repository management and collaboration
- Subscription system implementation for premium features
- Enhanced project card UI with 5-button layout (Open, Pull, Push, Save, Delete)
- GitHub repository browser opening functionality integrated with project cards
- Cross-platform browser support for Windows, macOS, and Linux
- Improved error handling for GitHub API integration and token management

## Next Development Priorities
1. **Testing and Validation**: Comprehensive testing of all API endpoints and UI workflows
2. **Error Handling**: Improved error handling and user feedback mechanisms
3. **Performance Optimization**: Database query optimization and UI responsiveness
4. **Security Enhancement**: Token management, input validation, and access control
5. **Documentation**: API documentation and user guides

## Active Decisions and Considerations
- **Database Choice**: MariaDB for production with MySQL fallback compatibility
- **UI Framework**: Avalonia for cross-platform desktop application support
- **AI Integration**: Custom prompt-based system for code explanation and transformation
- **Authentication**: Simple email/password system with role-based access
- **API Design**: RESTful architecture with JSON serialization

## Important Patterns and Preferences
- **Entity Relationships**: JPA/Hibernate with proper cascade operations
- **DTO Pattern**: Data Transfer Objects for API communication
- **Repository Pattern**: Spring Data JPA repositories for data access
- **MVVM Pattern**: Model-View-ViewModel for frontend architecture
- **Async Operations**: Asynchronous API calls in the frontend

## Technical Debt and Known Issues
- **Configuration Management**: Hardcoded API endpoints need environment-specific configuration
- **Logging**: Limited logging implementation for debugging and monitoring
- **Validation**: Input validation needs enhancement across the application
- **Testing**: Unit and integration test coverage needs improvement

## Development Environment Setup
- **Backend**: Java 21, Maven 3.x, MariaDB/MySQL database
- **Frontend**: .NET 10.0, Avalonia UI framework
- **Development Tools**: IntelliJ IDEA/Eclipse for backend, Visual Studio/Rider for frontend
- **Version Control**: Git with GitHub integration
