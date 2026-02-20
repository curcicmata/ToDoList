# Architecture Documentation

## Overview

ToDoList API is built using Clean Architecture principles, ensuring separation of concerns, testability, and maintainability. The application is designed to be scalable, extensible, and easy to understand.

## Clean Architecture Layers

### 1. Domain Layer (`ToDoList.Domain`)

The innermost layer containing enterprise business rules and domain entities. This layer has no dependencies on other layers.

**Key Components:**
- **Entities**: Core business objects (User, TodoTask, Category)
- **Enums**: Domain-level enumerations (TaskStatus, TaskPriority, UserRole)
- **Interfaces**: Repository contracts (IUserRepository, ICategoryRepository, ITodoTaskRepository)

**Responsibilities:**
- Define core business entities
- Establish business rules and invariants
- Provide repository interfaces

**Dependencies:** None

**Example:**
```csharp
public class TodoTask
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    // ... other properties
}
```

---

### 2. Application Layer (`ToDoList.Application`)

Contains application-specific business rules and orchestration logic. Depends only on the Domain layer.

**Key Components:**
- **DTOs**: Data Transfer Objects for API communication
- **Services**: Application services implementing business logic
- **Interfaces**: Service contracts
- **Validators**: FluentValidation validators for input validation

**Responsibilities:**
- Implement use cases
- Coordinate between UI and Domain
- Transform domain entities to DTOs
- Validate input data

**Dependencies:** Domain layer only

**Example:**
```csharp
public class TodoTaskService : ITodoTaskService
{
    private readonly ITodoTaskRepository _taskRepository;
    private readonly ICategoryRepository _categoryRepository;

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId)
    {
        // Validation and business logic
        var task = new TodoTask { ... };
        var created = await _taskRepository.CreateAsync(task);
        return MapToDto(created);
    }
}
```

---

### 3. Infrastructure Layer (`ToDoList.Infrastructure`)

Contains implementations of external concerns and technologies. Depends on Domain and Application layers.

**Key Components:**
- **Data**: EF Core DbContext, entity configurations, database seeding
- **Repositories**: Concrete implementations of repository interfaces
- **Services**: External service implementations (JWT token service)
- **BackgroundJobs**: Hangfire job implementations

**Responsibilities:**
- Database access via Entity Framework Core
- External service integration
- Background job processing
- Infrastructure-specific implementations

**Dependencies:** Domain, Application

**Example:**
```csharp
public class TodoTaskRepository : ITodoTaskRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<TodoTask> CreateAsync(TodoTask task)
    {
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }
}
```

---

### 4. API/Presentation Layer (`ToDoList.API`)

The outermost layer handling HTTP concerns and user interface. Depends on all inner layers.

**Key Components:**
- **Controllers**: API endpoints
- **Middleware**: Global exception handling
- **Filters**: Hangfire authorization
- **Program.cs**: Application bootstrap and configuration

**Responsibilities:**
- Handle HTTP requests/responses
- Authentication and authorization
- API documentation (Swagger)
- Error handling
- Dependency injection configuration

**Dependencies:** All layers

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITodoTaskService _taskService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskStatus? status)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllByUserIdAsync(userId, status);
        return Ok(tasks);
    }
}
```

---

## Dependency Flow

```
┌─────────────────────────────────────────────────┐
│              ToDoList.API                       │
│  (Controllers, Middleware, Configuration)       │
└────────────────┬────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────┐
│         ToDoList.Infrastructure                 │
│  (EF Core, Repositories, Background Jobs)       │
└────────────────┬────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────┐
│          ToDoList.Application                   │
│     (Services, DTOs, Validators)                │
└────────────────┬────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────┐
│            ToDoList.Domain                      │
│      (Entities, Interfaces, Enums)              │
└─────────────────────────────────────────────────┘
```

**Dependency Rule:** Dependencies flow inward. Inner layers know nothing about outer layers.

---

## Design Patterns

### 1. Repository Pattern
Abstracts data access logic, providing a collection-like interface for domain entities.

**Benefits:**
- Separation of concerns
- Testability (easy to mock)
- Centralized data access logic

**Implementation:**
```csharp
public interface ITodoTaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TodoTask>> GetAllByUserIdAsync(Guid userId);
    Task<TodoTask> CreateAsync(TodoTask task);
    Task<TodoTask> UpdateAsync(TodoTask task);
    Task DeleteAsync(Guid id, Guid userId);
}
```

### 2. Service Layer Pattern
Encapsulates business logic in service classes.

**Benefits:**
- Reusable business logic
- Clear separation from controllers
- Easier testing

### 3. Dependency Injection
All dependencies are injected through constructors.

**Benefits:**
- Loose coupling
- Testability
- Flexibility

**Configuration:**
```csharp
builder.Services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();
```

### 4. DTO Pattern
Data Transfer Objects separate internal domain models from API contracts.

**Benefits:**
- API stability
- Security (don't expose internal structure)
- Flexibility in data transformation

---

## Key Technologies

### Entity Framework Core
- **Purpose**: Object-Relational Mapping (ORM)
- **Database**: PostgreSQL
- **Features**: Migrations, LINQ queries, change tracking

### Hangfire
- **Purpose**: Background job processing
- **Storage**: PostgreSQL
- **Features**: Recurring jobs, job scheduling, dashboard

### JWT Authentication
- **Purpose**: Stateless authentication
- **Library**: Microsoft.AspNetCore.Authentication.JwtBearer
- **Token Expiration**: 60 minutes

### FluentValidation
- **Purpose**: Input validation
- **Benefits**: Readable, testable, maintainable validation rules

### Serilog
- **Purpose**: Structured logging
- **Sinks**: Console, File
- **Features**: Request logging, structured data

---

## Database Schema

### Core Tables

#### Users
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL,
    deleted_at TIMESTAMP
);
```

#### Categories
```sql
CREATE TABLE categories (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    color VARCHAR(7) NOT NULL,
    user_id UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL,
    deleted_at TIMESTAMP
);
```

#### TodoTasks
```sql
CREATE TABLE todo_tasks (
    id UUID PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status INTEGER NOT NULL,
    priority INTEGER NOT NULL,
    due_date TIMESTAMP,
    completed_at TIMESTAMP,
    user_id UUID NOT NULL REFERENCES users(id),
    category_id UUID REFERENCES categories(id),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL,
    deleted_at TIMESTAMP
);
```

### Hangfire Tables
Hangfire creates its own schema with tables for job storage, scheduling, and state management.

---

## Security

### Authentication
- **Method**: JWT Bearer tokens
- **Token Generation**: On login/registration
- **Token Validation**: Middleware validates on each request
- **Expiration**: 60 minutes

### Password Security
- **Algorithm**: BCrypt
- **Work Factor**: 11 (default)
- **Salt**: Automatically generated per password

### Authorization
- **Method**: Role-based (User, Admin)
- **Implementation**: `[Authorize]` attribute on controllers
- **User Context**: Claims-based identity

### Soft Delete
- **Purpose**: Data recovery, audit trail
- **Implementation**: `IsDeleted` and `DeletedAt` fields
- **Cleanup**: Background job removes old records after 30 days

---

## Background Jobs

### Architecture
```
┌──────────────┐
│  API Layer   │
│ (Triggers)   │
└──────┬───────┘
       │
       ↓
┌──────────────────────┐
│  Hangfire Server     │
│  (Job Processing)    │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐
│  Background Job      │
│  Service Layer       │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐
│  Repositories        │
│  (Data Access)       │
└──────────────────────┘
```

### Recurring Jobs

**Overdue Task Reminders**
- **Schedule**: Daily at 9:00 AM UTC
- **Purpose**: Identify and log overdue tasks
- **Integration Point**: Ready for notification system

**Cleanup Deleted Records**
- **Schedule**: Weekly on Sunday at 2:00 AM UTC
- **Purpose**: Permanently delete old soft-deleted records
- **Threshold**: 30 days

---

## API Design

### RESTful Principles
- **Resources**: Entities represented as resources (tasks, categories, users)
- **HTTP Methods**: GET (read), POST (create), PUT (update), DELETE (delete)
- **Status Codes**: Appropriate HTTP status codes for responses
- **Stateless**: Each request contains all necessary information


### Error Handling
- **Global Exception Middleware**: Catches all unhandled exceptions
- **Validation Errors**: Returned as 400 Bad Request with details
- **Not Found**: 404 with descriptive message
- **Unauthorized**: 401 for authentication failures

---

## Testing Strategy

### Unit Tests
- **Framework**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **Coverage**: Service layer (100%)

**Test Structure:**
```csharp
public class TodoTaskServiceTests
{
    private readonly Mock<ITodoTaskRepository> _mockRepository;
    private readonly TodoTaskService _service;

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateTask()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

---

## Performance Considerations

### Database
- **Indexes**: On foreign keys, user IDs, frequently queried fields
- **Pagination**: Prevent loading large datasets
- **Query Optimization**: Use `.AsNoTracking()` for read-only queries

### Caching
- **Future Enhancement**: Add response caching
- **Candidates**: Category lists, user profiles

### Background Jobs
- **Async Processing**: Offload long-running tasks
- **Retry Logic**: Hangfire automatic retries
- **Performance**: Runs on separate threads

---

## Scalability

### Horizontal Scaling
- **Stateless Design**: API instances can be added/removed
- **Shared Database**: All instances connect to same PostgreSQL
- **Hangfire**: Supports multiple server instances

### Vertical Scaling
- **Database**: PostgreSQL can handle large datasets
- **Connection Pooling**: EF Core manages connection pool

---

## Monitoring and Observability

### Logging
- **Serilog**: Structured logging with request/response logging
- **Log Levels**: Information, Warning, Error, Fatal
- **Persistence**: File-based logs with daily rolling

### Health Checks
- **Endpoint**: `/health`
- **Checks**: Database connectivity
- **Integration**: Ready for monitoring tools (Prometheus, etc.)

### Hangfire Dashboard
- **URL**: `/hangfire`
- **Features**: Job monitoring, manual triggers, failure analysis
- **Security**: Open in Development, authenticated in Production

---

## Deployment Architecture

### Docker Container
```
┌─────────────────────────────────┐
│     Docker Container            │
│  ┌───────────────────────────┐  │
│  │   ToDoList API            │  │
│  │   (.NET 8 Runtime)        │  │
│  └───────────┬───────────────┘  │
│              │                   │
└──────────────┼───────────────────┘
               │
               ↓
┌──────────────────────────────────┐
│     PostgreSQL Container         │
│   (Data Persistence)             │
└──────────────────────────────────┘
```

### CI/CD Pipeline
```
GitHub Push
    │
    ↓
GitHub Actions
    │
    ├─→ Build & Test
    │
    ├─→ Code Coverage
    │
    └─→ Docker Build & Push (main branch)
        │
        ↓
    Docker Registry
```