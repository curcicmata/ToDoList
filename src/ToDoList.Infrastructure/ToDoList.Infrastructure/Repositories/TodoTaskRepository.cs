using Microsoft.EntityFrameworkCore;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;
using ToDoList.Infrastructure.Data;

namespace ToDoList.Infrastructure.Repositories;

public class TodoTaskRepository(ApplicationDbContext context) : ITodoTaskRepository
{
    public async Task<TodoTask?> GetByIdAsync(Guid id, Guid userId)
    {
        return await context.TodoTasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<IEnumerable<TodoTask>> GetAllByUserIdAsync(Guid userId, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null, string? search = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null, bool includeArchived = false)
    {
        var query = context.TodoTasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!includeArchived)
            query = query.Where(t => !t.IsArchived && t.Status != ToDoList.Domain.Entities.TaskStatus.Completed);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.ILike(t.Title, $"%{search}%"));

        if (dueDateFrom.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= dueDateTo.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<TodoTask> Tasks, int TotalCount)> GetPagedAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        ToDoList.Domain.Entities.TaskStatus? status = null,
        Guid? categoryId = null,
        string? sortBy = null,
        bool sortDescending = false,
        string? search = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        bool includeArchived = false)
    {
        var query = context.TodoTasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!includeArchived)
            query = query.Where(t => !t.IsArchived && t.Status != ToDoList.Domain.Entities.TaskStatus.Completed);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.ILike(t.Title, $"%{search}%"));

        if (dueDateFrom.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= dueDateTo.Value);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "createdat" => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var tasks = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    public async Task<TodoTask> CreateAsync(TodoTask task)
    {
        context.TodoTasks.Add(task);
        await context.SaveChangesAsync();
        return task;
    }

    public async Task<TodoTask> UpdateAsync(TodoTask task)
    {
        context.TodoTasks.Update(task);
        await context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task != null)
        {
            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId)
    {
        return await context.TodoTasks
            .AnyAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<int> GetOverdueCountAsync(Guid userId)
    {
        return await context.TodoTasks
            .Where(t => t.UserId == userId
                && t.Status != ToDoList.Domain.Entities.TaskStatus.Completed
                && t.DueDate.HasValue
                && t.DueDate.Value.Date < DateTime.UtcNow.Date)
            .CountAsync();
    }

    public async Task ArchiveAsync(Guid id, Guid userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {id} not found");

        task.IsArchived = true;
        task.ArchivedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoTask>> GetArchivedAsync(Guid userId)
    {
        return await context.TodoTasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted && (t.IsArchived || t.Status == ToDoList.Domain.Entities.TaskStatus.Completed))
            .OrderByDescending(t => t.ArchivedAt ?? t.CompletedAt ?? t.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoTask>> GetTasksDueForArchiveAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-3);
        return await context.TodoTasks
            .Where(t => !t.IsDeleted
                && !t.IsArchived
                && t.Status == ToDoList.Domain.Entities.TaskStatus.Pending
                && t.DueDate.HasValue
                && t.DueDate.Value <= cutoff)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoTask>> GetTasksDueForReminderAsync()
    {
        return await context.TodoTasks
            .Include(t => t.User)
            .Where(t => !t.IsDeleted
                && !t.ReminderSent
                && t.ReminderTime.HasValue
                && t.ReminderTime.Value <= DateTime.UtcNow)
            .ToListAsync();
    }
}
