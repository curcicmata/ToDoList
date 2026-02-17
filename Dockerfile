# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ToDoList.sln ./
COPY src/ToDoList.Domain/ToDoList.Domain/ToDoList.Domain.csproj ./src/ToDoList.Domain/ToDoList.Domain/
COPY src/ToDoList.Application/ToDoList.Application/ToDoList.Application.csproj ./src/ToDoList.Application/ToDoList.Application/
COPY src/ToDoList.Infrastructure/ToDoList.Infrastructure/ToDoList.Infrastructure.csproj ./src/ToDoList.Infrastructure/ToDoList.Infrastructure/
COPY src/ToDoList.API/ToDoList.API/ToDoList.API.csproj ./src/ToDoList.API/ToDoList.API/
COPY tests/ToDoList.UnitTests/ToDoList.UnitTests/ToDoList.UnitTests.csproj ./tests/ToDoList.UnitTests/ToDoList.UnitTests/
COPY tests/ToDoList.IntegrationTests/ToDoList.IntegrationTests/ToDoList.IntegrationTests.csproj ./tests/ToDoList.IntegrationTests/ToDoList.IntegrationTests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build the project
WORKDIR /src/src/ToDoList.API/ToDoList.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoList.API.dll"]
