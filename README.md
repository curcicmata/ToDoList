# ToDoList API

A modern, scalable To-Do application built with ASP.NET Core 8, following Clean Architecture principles. This global application helps users organize and manage tasks efficiently with features like categorization, deadlines, reminders, and automatic archiving.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── ToDoList.Domain/          # Enterprise business rules and entities
│   ├── Entities/             # Domain entities (Task, Category, User)
│   ├── Enums/                # Domain enumerations
│   └── Interfaces/           # Domain interfaces
│
├── ToDoList.Application/     # Application business rules
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Services/             # Application services
│   ├── Interfaces/           # Service interfaces
│   └── Validators/           # Input validation logic
│
├── ToDoList.Infrastructure/  # External concerns (DB, Email, etc.)
│   ├── Data/                 # EF Core DbContext and configurations
│   ├── Repositories/         # Data access implementations
│   └── Services/             # External service implementations
│
└── ToDoList.API/             # Presentation layer
    ├── Controllers/          # API endpoints
    └── Middleware/           # Global exception handler

tests/
├── ToDoList.UnitTests/       # Unit tests for business logic
└── ToDoList.IntegrationTests/ # Integration tests for API endpoints
```

## 🚀 Tech Stack

- **Framework**: ASP.NET Core 8.0 (Web API)
- **Language**: C# with .NET 8
- **Database**: PostgreSQL with Entity Framework Core (Code-First)
- **Authentication**: JWT Bearer tokens
- **Background Jobs**: Hangfire (for email reminders and auto-archiving)
- **Logging**: Serilog (Console + File)
- **API Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Testing**: xUnit with integration test support
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions (planned)

## ✨ Features

### User Management
- User registration with email and password
- JWT-based authentication
- Role-based authorization

### Task Management
- Create, update, delete, and retrieve tasks
- Task properties:
  - Title/Summary and description
  - Status tracking (Pending, In Progress, Completed)
  - Priority levels (Low, Medium, High)
  - Optional deadlines with date and time
  - Optional reminder notifications
- Pagination support for task listings
- Search by task title
- Filter by deadline date
- Sort by creation date, deadline, or priority
- Soft delete support

### Categories
- Create, update, and delete custom categories
- Assign optional color or icon identifiers
- Associate tasks with categories

### Background Processing
- **Email Reminders**: Automated email notifications sent at reminder time
- **Auto-Archiving**: Pending tasks are automatically archived 3 days after deadline expiry

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

## 🛠️ Getting Started

### Option 1: Run Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/curcicmata/ToDoList.git
   cd ToDoList
   ```

2. **Set up PostgreSQL**
   - Install PostgreSQL locally
   - Create a database named `todolist_dev`
   - Update connection string in `src/ToDoList.API/ToDoList.API/appsettings.Development.json`

3. **Apply database migrations** (once EF Core migrations are created)
   ```bash
   cd src/ToDoList.API/ToDoList.API
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   Navigate to: `https://localhost:5001/swagger`

### Option 2: Run with Docker Compose

1. **Clone the repository**
   ```bash
   git clone https://github.com/curcicmata/ToDoList.git
   cd ToDoList
   ```

2. **Start services**
   ```bash
   docker-compose up -d
   ```

3. **Access the services**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`
   - PgAdmin: `http://localhost:5050` (admin@todolist.com / admin)

4. **Stop services**
   ```bash
   docker-compose down
   ```

## 🧪 Running Tests

### Run all tests
```bash
dotnet test
```

### Run unit tests only
```bash
dotnet test tests/ToDoList.UnitTests/ToDoList.UnitTests/ToDoList.UnitTests.csproj
```

### Run integration tests only
```bash
dotnet test tests/ToDoList.IntegrationTests/ToDoList.IntegrationTests/ToDoList.IntegrationTests.csproj
```

## 📝 Configuration

### Connection Strings
Update `appsettings.json` or `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=todolist_dev;Username=postgres;Password=postgres"
  }
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ToDoListAPI",
    "Audience": "ToDoListClient",
    "ExpiryInMinutes": 60
  }
}
```

### Logging (Serilog)
Logs are written to:
- Console output
- `logs/todolist-{Date}.log` files

## 🔧 Development

### Build the solution
```bash
dotnet build
```

### Restore packages
```bash
dotnet restore
```

### Clean build artifacts
```bash
dotnet clean
```

## 📦 Project Status

✅ **Task 1: Project Setup & Infrastructure** - COMPLETED
- Clean Architecture solution structure
- PostgreSQL with Entity Framework Core setup
- Serilog structured logging
- Global exception handling middleware
- Swagger/OpenAPI documentation
- Docker containerization

🔄 **Upcoming Tasks**:
- Task 2: Authentication & User Management
- Task 3: Core Domain - Task & Category Management
- Task 4: Background Jobs with Hangfire
- Task 5: Testing & Quality Assurance
- Task 6: CI/CD, Deployment & Documentation

## 📄 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.