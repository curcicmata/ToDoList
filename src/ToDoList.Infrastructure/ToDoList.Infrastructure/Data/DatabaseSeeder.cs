using Microsoft.EntityFrameworkCore;
using ToDoList.Domain.Entities;

namespace ToDoList.Infrastructure.Data;

public class DatabaseSeeder(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            return;
        }

        // Create test users
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "john.doe@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = ToDoList.Domain.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "jane.smith@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = ToDoList.Domain.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Create categories for user1
        var categories1 = new List<Category>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Work",
                Description = "Work-related tasks",
                Color = "#FF5733",
                UserId = user1.Id,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Personal",
                Description = "Personal errands and activities",
                Color = "#33C3FF",
                UserId = user1.Id,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Shopping",
                Description = "Shopping list",
                Color = "#75FF33",
                UserId = user1.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Create categories for user2
        var categories2 = new List<Category>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Projects",
                Description = "Project tasks",
                Color = "#FF33F5",
                UserId = user2.Id,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Health",
                Description = "Health and fitness",
                Color = "#FFD433",
                UserId = user2.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Categories.AddRange(categories1);
        _context.Categories.AddRange(categories2);
        await _context.SaveChangesAsync();

        // Create tasks for user1
        var tasks1 = new List<TodoTask>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Complete project proposal",
                Description = "Finish the Q1 project proposal document",
                Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
                Priority = ToDoList.Domain.Entities.TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(3),
                UserId = user1.Id,
                CategoryId = categories1[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Team meeting preparation",
                Description = "Prepare slides for weekly team sync",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = ToDoList.Domain.Entities.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(1),
                UserId = user1.Id,
                CategoryId = categories1[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Buy groceries",
                Description = "Milk, eggs, bread, vegetables",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = ToDoList.Domain.Entities.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(2),
                UserId = user1.Id,
                CategoryId = categories1[2].Id,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Schedule dentist appointment",
                Description = "Call dentist office for checkup",
                Status = ToDoList.Domain.Entities.TaskStatus.Completed,
                Priority = ToDoList.Domain.Entities.TaskPriority.Low,
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                UserId = user1.Id,
                CategoryId = categories1[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Review code PRs",
                Description = "Review pending pull requests from team",
                Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
                Priority = ToDoList.Domain.Entities.TaskPriority.Urgent,
                DueDate = DateTime.UtcNow.AddHours(4),
                UserId = user1.Id,
                CategoryId = categories1[0].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        // Create tasks for user2
        var tasks2 = new List<TodoTask>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Design new landing page",
                Description = "Create mockups for the new product landing page",
                Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
                Priority = ToDoList.Domain.Entities.TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(7),
                UserId = user2.Id,
                CategoryId = categories2[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Update API documentation",
                Description = "Document the new endpoints added this sprint",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = ToDoList.Domain.Entities.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(5),
                UserId = user2.Id,
                CategoryId = categories2[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Morning workout",
                Description = "30 min cardio and strength training",
                Status = ToDoList.Domain.Entities.TaskStatus.Completed,
                Priority = ToDoList.Domain.Entities.TaskPriority.High,
                CompletedAt = DateTime.UtcNow.AddHours(-8),
                UserId = user2.Id,
                CategoryId = categories2[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Prep meal for the week",
                Description = "Cook and portion meals for next 5 days",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = ToDoList.Domain.Entities.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(1),
                UserId = user2.Id,
                CategoryId = categories2[1].Id,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Fix bug #42",
                Description = "Investigate and fix the login timeout issue",
                Status = ToDoList.Domain.Entities.TaskStatus.Cancelled,
                Priority = ToDoList.Domain.Entities.TaskPriority.Low,
                UserId = user2.Id,
                CategoryId = categories2[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        _context.TodoTasks.AddRange(tasks1);
        _context.TodoTasks.AddRange(tasks2);
        await _context.SaveChangesAsync();
    }
}
