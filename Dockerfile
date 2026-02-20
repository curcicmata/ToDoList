# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY src/ToDoList.Domain/ToDoList.Domain/ToDoList.Domain.csproj ./src/ToDoList.Domain/ToDoList.Domain/
COPY src/ToDoList.Application/ToDoList.Application/ToDoList.Application.csproj ./src/ToDoList.Application/ToDoList.Application/
COPY src/ToDoList.Infrastructure/ToDoList.Infrastructure/ToDoList.Infrastructure.csproj ./src/ToDoList.Infrastructure/ToDoList.Infrastructure/
COPY src/ToDoList.API/ToDoList.API/ToDoList.API.csproj ./src/ToDoList.API/ToDoList.API/

# Restore dependencies
RUN dotnet restore src/ToDoList.API/ToDoList.API/ToDoList.API.csproj

# Copy all source files
COPY . .

# Build and publish the project
WORKDIR /src/src/ToDoList.API/ToDoList.API
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create a non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ToDoList.API.dll"]
