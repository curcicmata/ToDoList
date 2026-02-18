using ToDoList.Application.DTOs;

namespace ToDoList.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
