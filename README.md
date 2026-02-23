# ToDoList API

A modern, scalable To-Do application built with ASP.NET Core 9, following Clean Architecture principles. This global application helps users organize and manage tasks efficiently with features like categorization, deadlines, reminders, and automatic archiving.

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
Detailed information about architecture used here can be found here - [Architecture guide](https://github.com/curcicmata/ToDoList/blob/main/ARCHITECTURE.md)

## API guide
API guide can be found here - [API guide](https://github.com/curcicmata/ToDoList/blob/main/API.md)


## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

## Getting Started

Info about starting the application can be found here - [Setup](https://github.com/curcicmata/ToDoList/blob/main/SETUP.md)

## Swagger 
To open the swagger follow this link - [Swagger guide](https://github.com/curcicmata/ToDoList/blob/main/SWAGGER_GUIDE.md)

## Configuration

### Logging (Serilog)
Logs are written to:
- Console output
- `logs/todolist-{Date}.log` files

## Development

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
