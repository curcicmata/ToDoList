using Microsoft.EntityFrameworkCore;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;
using ToDoList.Infrastructure.Data;

namespace ToDoList.Infrastructure.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, Guid userId)
    {
        return await context.Categories
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId)
    {
        return await context.Categories
            .Include(c => c.Tasks)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var category = await GetByIdAsync(id, userId);
        if (category != null)
        {
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId)
    {
        return await context.Categories
            .AnyAsync(c => c.Id == id && c.UserId == userId);
    }
}
