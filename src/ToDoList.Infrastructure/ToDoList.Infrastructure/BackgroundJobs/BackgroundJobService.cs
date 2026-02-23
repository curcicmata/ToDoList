using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.Application.Interfaces;
using ToDoList.Domain.Interfaces;
using ToDoList.Infrastructure.Data;

namespace ToDoList.Infrastructure.BackgroundJobs;

public class BackgroundJobService(ApplicationDbContext context, ILogger<BackgroundJobService> logger, ITodoTaskRepository taskRepository, IEmailService emailService) : IBackgroundJobService
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

    public async Task SendTaskReminders()
    {
        logger.LogInformation("Starting task reminder email job at {Time}", DateTime.UtcNow);

        try
        {
            var tasks = await taskRepository.GetTasksDueForReminderAsync();
            var taskList = tasks.ToList();

            logger.LogInformation("Found {Count} tasks due for reminders", taskList.Count);

            foreach (var task in taskList)
            {
                try
                {
                    var subject = $"Reminder: Task '{task.Title}' is due soon";
                    var body = $"This is a reminder that your task '{task.Title}' has a reminder set for {task.ReminderTime:yyyy-MM-dd HH:mm} UTC." +
                               (task.DueDate.HasValue ? $"\nDue date: {task.DueDate:yyyy-MM-dd}" : string.Empty);

                    await emailService.SendEmailAsync(task.User.Email, subject, body);

                    task.ReminderSent = true;
                    await taskRepository.UpdateAsync(task);

                    logger.LogInformation("Reminder sent for task '{Title}' (ID: {TaskId}) to {Email}", task.Title, task.Id, task.User.Email);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send reminder for task {TaskId}", task.Id);
                }
            }

            logger.LogInformation("Completed task reminder email job");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while sending task reminders");
            throw;
        }
    }

    public async Task ArchiveOverdueTasks()
    {
        logger.LogInformation("Starting auto-archive overdue tasks job at {Time}", DateTime.UtcNow);

        try
        {
            var tasks = await taskRepository.GetTasksDueForArchiveAsync();
            var taskList = tasks.ToList();

            logger.LogInformation("Found {Count} tasks to auto-archive", taskList.Count);

            foreach (var task in taskList)
            {
                task.IsArchived = true;
                task.ArchivedAt = DateTime.UtcNow;
                await taskRepository.UpdateAsync(task);
                logger.LogInformation("Auto-archived task '{Title}' (ID: {TaskId})", task.Title, task.Id);
            }

            logger.LogInformation("Completed auto-archive overdue tasks job");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while archiving overdue tasks");
            throw;
        }
    }
}
