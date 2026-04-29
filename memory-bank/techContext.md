# CodeAI Tools Tech Context

## Technology Stack

### Backend Technologies
- **Framework**: Spring Boot 3.4.2
- **Language**: Java 21
- **Build Tool**: Maven
- **Database**: MariaDB (primary) with MySQL compatibility
- **ORM**: JPA/Hibernate
- **API**: RESTful with JSON serialization
- **External Libraries**:
  - Lombok for boilerplate reduction
  - MapStruct for object mapping
  - GitHub API for repository integration
  - Jakarta Persistence for database operations

### Frontend Technologies
- **Framework**: Avalonia UI 11.3.12
- **Language**: C# (.NET 10.0)
- **Architecture**: MVVM pattern
- **UI Components**: Avalonia.AvaloniaEdit for code editing
- **External Libraries**:
  - LibGit2Sharp for Git operations
  - Octokit for GitHub API
  - Newtonsoft.Json for JSON serialization
  - MsBox.Avalonia for message boxes

## Development Setup

### Backend Development
- **IDE**: IntelliJ IDEA or Eclipse
- **JDK**: OpenJDK 21
- **Database**: MariaDB server instance
- **Build**: Maven wrapper (`mvnw`) for consistent builds
- **Testing**: Spring Boot Test framework

### Frontend Development
- **IDE**: Visual Studio 2022 or JetBrains Rider
- **Runtime**: .NET 10.0 runtime
- **Build**: MSBuild with project file configuration
- **Designer**: Avalonia visual designer support

## Database Configuration
- **Connection**: Configured via application.properties
- **Entities**: JPA annotations for table mapping
- **Relationships**: Proper foreign key constraints and cascade operations
- **Migrations**: Manual schema management (no Flyway/Liquibase currently)

## API Architecture
- **Base URL**: http://localhost:8080
- **Content-Type**: application/json
- **Authentication**: Session-based with user roles
- **Error Handling**: HTTP status codes with descriptive messages

## Key Dependencies

### Backend Dependencies
```xml
- spring-boot-starter-web: Web MVC framework
- spring-boot-starter-data-jpa: Database operations
- lombok: Code generation
- mapstruct: Object mapping
- github-api: GitHub integration
- mariadb-java-client: Database driver
```

### Frontend Dependencies
```xml
- Avalonia: UI framework
- Avalonia.AvaloniaEdit: Code editor component
- LibGit2Sharp: Git operations
- Octokit: GitHub API client
- Newtonsoft.Json: JSON handling
- MsBox.Avalonia: Message dialogs
``## Development Tools and Utilities
- **Version Control**: Git with GitHub integration
- **API Testing**: Postman or similar tools
- **Database Management**: HeidiSQL or similar MariaDB client
- **Code Quality**: Built-in IDE analysis tools

## Configuration Management
- **Backend**: application.properties for database and server settings
- **Frontend**: Consts.cs for API endpoints and configuration values
- **Environment**: Development environment with localhost endpoints

## Deployment Considerations
- **Backend**: Spring Boot JAR deployment with embedded Tomcat
- **Frontend**: Self-contained desktop executable via .NET publishing
- **Database**: External MariaDB/MySQL server required
- **Networking**: HTTP communication between frontend and backend

## Performance Considerations
- **Database**: Connection pooling via Spring Boot defaults
- **API**: Asynchronous operations in frontend to prevent UI blocking
- **Memory**: Entity lazy loading to minimize memory usage
- **Caching**: No current caching implementation (potential optimization area)
