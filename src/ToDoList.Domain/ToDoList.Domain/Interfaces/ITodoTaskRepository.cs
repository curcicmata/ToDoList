using ToDoList.Domain.Entities;

namespace ToDoList.Domain.Interfaces;

public interface ITodoTaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TodoTask>> GetAllByUserIdAsync(Guid userId, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null);
    Task<(IEnumerable<TodoTask> Tasks, int TotalCount)> GetPagedAsync(Guid userId, int pageNumber, int pageSize, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null, string? sortBy = null, bool sortDescending = false);
    Task<TodoTask> CreateAsync(TodoTask task);
    Task<TodoTask> UpdateAsync(TodoTask task);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid id, Guid userId);
    Task<int> GetOverdueCountAsync(Guid userId);
}
