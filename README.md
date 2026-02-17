# ToDoList API

A modern, scalable To-Do application built with ASP.NET Core 8, following Clean Architecture principles. This global application helps users organize and manage tasks efficiently with features like categorization, deadlines, reminders, and automatic archiving.

## Architecture

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

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

## Getting Started

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

## Running Tests

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

## Configuration

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
