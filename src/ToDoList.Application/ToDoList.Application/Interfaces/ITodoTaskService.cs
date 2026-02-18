using ToDoList.Application.DTOs;

namespace ToDoList.Application.Interfaces;

public interface ITodoTaskService
{
    Task<TodoTaskDto?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TodoTaskDto>> GetAllByUserIdAsync(Guid userId, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null);
    Task<PagedResultDto<TodoTaskDto>> GetPagedAsync(Guid userId, int pageNumber, int pageSize, ToDoList.Domain.Entities.TaskStatus? status = null, Guid? categoryId = null, string? sortBy = null, bool sortDescending = false);
    Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId);
    Task<TodoTaskDto> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<int> GetOverdueCountAsync(Guid userId);
}
