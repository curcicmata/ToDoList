using ToDoList.Domain.Entities;

namespace ToDoList.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid id, Guid userId);
}
