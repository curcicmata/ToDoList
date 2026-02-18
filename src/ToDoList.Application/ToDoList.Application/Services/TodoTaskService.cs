using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;

namespace ToDoList.Application.Services;

public class TodoTaskService(ITodoTaskRepository taskRepository, ICategoryRepository categoryRepository) : ITodoTaskService
{
    public async Task<TodoTaskDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(id, userId);
        return task == null ? null : MapToDto(task);
    }

    public async Task<IEnumerable<TodoTaskDto>> GetAllByUserIdAsync(Guid userId, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null)
    {
        var tasks = await taskRepository.GetAllByUserIdAsync(userId, status, categoryId);
        return tasks.Select(MapToDto);
    }

    public async Task<PagedResultDto<TodoTaskDto>> GetPagedAsync(Guid userId, int pageNumber, int pageSize, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null, string? sortBy = null, bool sortDescending = false)
    {
        var (tasks, totalCount) = await taskRepository.GetPagedAsync(userId, pageNumber, pageSize, status, categoryId, sortBy, sortDescending);

        return new PagedResultDto<TodoTaskDto>
        {
            Items = tasks.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId)
    {
        // Validate category if provided
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await categoryRepository.ExistsAsync(dto.CategoryId.Value, userId);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {dto.CategoryId.Value} not found");
        }

        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            CategoryId = dto.CategoryId,
            UserId = userId,
            Status = ToDoList.Domain.Entities.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await taskRepository.CreateAsync(task);
        return MapToDto(created);
    }

    public async Task<TodoTaskDto> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(id, userId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {id} not found");

        // Validate category if provided
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await categoryRepository.ExistsAsync(dto.CategoryId.Value, userId);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {dto.CategoryId.Value} not found");
        }

        // Track status changes
        var previousStatus = task.Status;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.CategoryId = dto.CategoryId;
        task.UpdatedAt = DateTime.UtcNow;

        // Set completion time if status changed to Completed
        if (previousStatus != ToDoList.Domain.Entities.TaskStatus.Completed && dto.Status == ToDoList.Domain.Entities.TaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.Status != ToDoList.Domain.Entities.TaskStatus.Completed)
        {
            task.CompletedAt = null;
        }

        var updated = await taskRepository.UpdateAsync(task);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await taskRepository.DeleteAsync(id, userId);
    }

    public async Task<int> GetOverdueCountAsync(Guid userId)
    {
        return await taskRepository.GetOverdueCountAsync(userId);
    }

    private static TodoTaskDto MapToDto(TodoTask task)
    {
        return new TodoTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CategoryId = task.CategoryId,
            CategoryName = task.Category?.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
