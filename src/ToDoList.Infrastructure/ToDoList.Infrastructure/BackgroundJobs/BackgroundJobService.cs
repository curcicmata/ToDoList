using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.Infrastructure.Data;

namespace ToDoList.Infrastructure.BackgroundJobs;

public class BackgroundJobService(ApplicationDbContext context, ILogger<BackgroundJobService> logger) : IBackgroundJobService
{
    public void SendOverdueTaskReminders()
    {
        logger.LogInformation("Starting overdue task reminders job at {Time}", DateTime.UtcNow);

        try
        {
            var overdueTasks = context.TodoTasks
                .Include(t => t.User)
                .Where(t => !t.IsDeleted
                    && t.Status != ToDoList.Domain.Entities.TaskStatus.Completed
                    && t.Status != ToDoList.Domain.Entities.TaskStatus.Cancelled
                    && t.DueDate.HasValue
                    && t.DueDate.Value.Date < DateTime.UtcNow.Date)
                .ToList();

            logger.LogInformation("Found {Count} overdue tasks", overdueTasks.Count);

            foreach (var task in overdueTasks)
            {
                // In a real application, send email/notification here
                logger.LogInformation(
                    "Reminder: Task '{Title}' (ID: {TaskId}) is overdue for user {Email}. Due date was {DueDate}",
                    task.Title,
                    task.Id,
                    task.User.Email,
                    task.DueDate);
            }

            logger.LogInformation("Completed overdue task reminders job");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while sending overdue task reminders");
            throw;
        }
    }

    public void CleanupSoftDeletedRecords()
    {
        logger.LogInformation("Starting cleanup of soft-deleted records at {Time}", DateTime.UtcNow);

        try
        {
            // Delete records soft-deleted more than 30 days ago
            var cutoffDate = DateTime.UtcNow.AddDays(-30); 

            // Hard delete old soft-deleted users
            var deletedUsers = context.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted && u.DeletedAt.HasValue && u.DeletedAt.Value < cutoffDate)
                .ToList();

            if (deletedUsers.Count != 0)
            {
                context.Users.RemoveRange(deletedUsers);
                logger.LogInformation("Permanently deleted {Count} users", deletedUsers.Count);
            }

            // Hard delete old soft-deleted categories
            var deletedCategories = context.Categories
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted && c.DeletedAt.HasValue && c.DeletedAt.Value < cutoffDate)
                .ToList();

            if (deletedCategories.Count != 0)
            {
                context.Categories.RemoveRange(deletedCategories);
                logger.LogInformation("Permanently deleted {Count} categories", deletedCategories.Count);
            }

            // Hard delete old soft-deleted tasks
            var deletedTasks = context.TodoTasks
                .IgnoreQueryFilters()
                .Where(t => t.IsDeleted && t.DeletedAt.HasValue && t.DeletedAt.Value < cutoffDate)
                .ToList();

            if (deletedTasks.Count != 0)
            {
                context.TodoTasks.RemoveRange(deletedTasks);
                logger.LogInformation("Permanently deleted {Count} tasks", deletedTasks.Count);
            }

            context.SaveChanges();
            logger.LogInformation("Completed cleanup of soft-deleted records");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while cleaning up soft-deleted records");
            throw;
        }
    }
}
