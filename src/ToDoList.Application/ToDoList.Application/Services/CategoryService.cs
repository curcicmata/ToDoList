using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;

namespace ToDoList.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var category = await categoryRepository.GetByIdAsync(id, userId);
        return category == null ? null : MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId)
    {
        var categories = await categoryRepository.GetAllByUserIdAsync(userId);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await categoryRepository.CreateAsync(category);
        return MapToDto(created);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, Guid userId)
    {
        var category = await categoryRepository.GetByIdAsync(id, userId);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {id} not found");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Color = dto.Color;
        category.UpdatedAt = DateTime.UtcNow;

        var updated = await categoryRepository.UpdateAsync(category);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await categoryRepository.DeleteAsync(id, userId);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            TaskCount = category.Tasks?.Count ?? 0,
            CreatedAt = category.CreatedAt
        };
    }
}
