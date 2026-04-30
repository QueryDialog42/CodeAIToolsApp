# CodeAI Tools System Patterns

## System Architecture
The CodeAI Tools platform follows a client-server architecture with clear separation of concerns:

### Backend Architecture (Spring Boot)
- **Layered Architecture**: Controller → Service → Repository → Entity
- **RESTful API Design**: Standard HTTP methods with JSON payloads
- **JPA/Hibernate ORM**: Database abstraction with entity relationships
- **Dependency Injection**: Spring IoC container for component management

### Frontend Architecture (Avalonia)
- **MVVM Pattern**: Model-View-ViewModel separation
- **User Control Composition**: Reusable UI components
- **Async/Await Pattern**: Non-blocking API communication
- **Event-Driven UI**: Reactive user interface updates

## Key Design Patterns

### Backend Patterns
- **Repository Pattern**: Spring Data JPA repositories for data access
- **DTO Pattern**: Data Transfer Objects for API communication
- **Service Layer Pattern**: Business logic separation from controllers
- **Entity Relationship Pattern**: JPA annotations for database mapping
- **Controller Pattern**: RESTful endpoint handling

### Frontend Patterns
- **MVVM Pattern**: ViewModels with data binding
- **Command Pattern**: UI action handling
- **Observer Pattern**: Property change notifications
- **Factory Pattern**: Window and control creation
- **Singleton Pattern**: Request manager and configuration

## Component Relationships

### Entity Relationships
- **Users**: One-to-Many with Projects, One-to-One with GitTokens
- **Projects**: Many-to-One with Users, contains Duties
- **Teams**: Many-to-Many relationship with Users
- **Duties**: Belong to Projects and Teams
- **Subscriptions**: One-to-One with Users

### API Communication Flow
1. **Frontend Request**: ViewModel → RequestManager → HTTP Client
2. **Backend Processing**: Controller → Service → Repository → Database
3. **Response Flow**: Database → Repository → Service → Controller → HTTP Response
4. **Frontend Handling**: HTTP Response → RequestManager → ViewModel → View Update

## Critical Implementation Paths

### User Authentication Flow
1. Login credentials entered in StartWindow
2. RequestManager sends login request to backend
3. Backend validates credentials and returns user role
4. Frontend opens appropriate window (Admin/Worker)

### Project Management Flow
1. Admin creates project through AdminWindow
2. Project data sent to ProjectController
3. Backend persists project with user relationship
4. UI updates to reflect new project

### AI Assistance Flow
1. Worker submits code for explanation/transformation
2. RequestManager sends code to AI endpoints
3. Backend processes with system prompts
4. AI response returned and displayed in UI

### GitHub Repository Access Flow
1. User clicks 'Open' button on project card
2. Frontend retrieves GitHub token from backend API
3. Frontend calls GitHub API to get username using token
4. Repository URL constructed (https://github.com/{username}/{project_name})
5. Cross-platform browser opens repository URL
6. Error handling for missing tokens or API failures

## Data Flow Patterns
- **Request-Response**: Synchronous API calls for most operations
- **Async Processing**: Non-blocking UI updates during API calls
- **Event Propagation**: Property change notifications for UI updates
- **State Management**: Centralized user session and project state

## Security Patterns
- **Role-Based Access Control**: Admin/Worker role validation
- **Token Management**: GitHub API token secure storage
- **Input Validation**: DTO validation and sanitization
- **API Endpoint Protection**: Role-based endpoint access
