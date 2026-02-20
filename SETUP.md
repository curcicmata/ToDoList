# Development Setup Guide

This guide will help you set up the ToDoList API for local development.

## Prerequisites

### Required Software
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/downloads)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Optional Software
- [Docker Desktop](https://www.docker.com/products/docker-desktop) - For containerized development
- [PgAdmin](https://www.pgadmin.org/) - PostgreSQL GUI (if not using Docker)
- [Postman](https://www.postman.com/) - API testing

## Step-by-Step Setup

### 1. Clone the Repository

```bash
git clone https://github.com/curcicmata/ToDoList.git
cd ToDoList
```

### 2. Database Setup

#### Option A: Local PostgreSQL

1. Install PostgreSQL 16+ on your system
2. Create a new database:
```sql
CREATE DATABASE todolist_db;
```

3. Update the connection string in `src/ToDoList.API/ToDoList.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=todolist_db;Username=postgres;Password=your_password"
  }
}
```

#### Option B: Docker PostgreSQL

```bash
docker run --name todolist-postgres \
  -e POSTGRES_DB=todolist_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:16-alpine
```

### 3. Configure JWT Settings

Update JWT settings in `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "ToDoListAPI",
    "Audience": "ToDoListClient"
  }
}
```

**Important**: Never commit real secrets to source control. Use User Secrets for sensitive data:

```bash
cd src/ToDoList.API/ToDoList.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### 4. Restore NuGet Packages

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Apply Database Migrations

The application will automatically run migrations on startup in Development mode. Alternatively, run manually:

```bash
cd src/ToDoList.API/ToDoList.API
dotnet ef database update
```

### 7. Run the Application

```bash
cd src/ToDoList.API/ToDoList.API
dotnet run
```

The API will start at:
- HTTPS: https://localhost:7207
- HTTP: http://localhost:5294

### 8. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:7207/swagger
```

## Project Structure

```
ToDoList/
├── src/
│   ├── ToDoList.Domain/
│   │   ├── Entities/              # Domain models
│   │   ├── Enums/                 # Enumerations
│   │   └── Interfaces/            # Repository interfaces
│   ├── ToDoList.Application/
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Interfaces/            # Service interfaces
│   │   ├── Services/              # Business logic
│   │   └── Validators/            # Input validators
│   ├── ToDoList.Infrastructure/
│   │   ├── Data/                  # DbContext & seeding
│   │   ├── Repositories/          # Data access
│   │   ├── Services/              # Infrastructure services
│   │   └── BackgroundJobs/        # Hangfire jobs
│   └── ToDoList.API/
│       ├── Controllers/           # API endpoints
│       ├── Middleware/            # Exception handling
│       └── Filters/               # Hangfire filters
└── tests/
    ├── ToDoList.UnitTests/        # Unit tests
    └── ToDoList.IntegrationTests/ # Integration tests (future)
```

## Running Tests

### Unit Tests
```bash
cd tests/ToDoList.UnitTests/ToDoList.UnitTests
dotnet test
```

### With Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Continuous Test Running (Watch Mode)
```bash
dotnet watch test
```

## Database Migrations

### Create a New Migration
```bash
cd src/ToDoList.Infrastructure/ToDoList.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ToDoList.API/ToDoList.API
```

### Apply Migrations
```bash
dotnet ef database update --startup-project ../ToDoList.API/ToDoList.API
```

### Remove Last Migration
```bash
dotnet ef migrations remove --startup-project ../ToDoList.API/ToDoList.API
```

## Seed Data

The application automatically seeds test data in Development mode:
- 2 test users (john.doe@example.com / jane.smith@example.com)
- 5 categories
- 10 sample tasks

Password for all test users: `Password123!`

## Background Jobs (Hangfire)

Access the Hangfire dashboard at:
```
https://localhost:7207/hangfire
```

In Development mode, no authentication is required.

### Configured Jobs
1. **Overdue Reminders**: Daily at 9:00 AM UTC
2. **Cleanup Deleted Records**: Weekly on Sunday at 2:00 AM UTC

## Troubleshooting

### Database Connection Issues
- Verify PostgreSQL is running: `psql -U postgres`
- Check connection string in appsettings.json
- Ensure database exists: `psql -U postgres -l`

### Port Already in Use
Edit `launchSettings.json` to change ports:
```json
"applicationUrl": "https://localhost:7208;http://localhost:5295"
```

### Migration Issues
Reset database and reapply migrations:
```bash
dotnet ef database drop --force --startup-project ../ToDoList.API/ToDoList.API
dotnet ef database update --startup-project ../ToDoList.API/ToDoList.API
```

### Hangfire Dashboard Not Loading
- Check that Hangfire services are registered
- Verify database connection
- Check Hangfire tables exist in database

## Environment Variables

For production deployments, use environment variables:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Database=todolist_db;Username=user;Password=pass"
export Jwt__Key="production-secret-key"
export Jwt__Issuer="ToDoListAPI"
export Jwt__Audience="ToDoListClient"
export ASPNETCORE_ENVIRONMENT="Production"
```

## Docker Development

### Build Docker Image
```bash
docker build -t todolist-api .
```

### Run with Docker Compose
```bash
docker-compose up -d
```

### View Logs
```bash
docker-compose logs -f todolist-api
```

### Stop Services
```bash
docker-compose down
```

### Rebuild After Changes
```bash
docker-compose up -d --build
```