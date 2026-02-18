using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;

namespace ToDoList.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var userId = GetUserId();
        var categories = await _categoryService.GetAllByUserIdAsync(userId);
        return Ok(categories);
    }

    /// <summary>
    /// Get category by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var category = await _categoryService.GetByIdAsync(id, userId);

        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    /// <summary>
    /// Create a category
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var userId = GetUserId();
        var category = await _categoryService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Update a category
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var userId = GetUserId();
            var category = await _categoryService.UpdateAsync(id, dto, userId);
            return Ok(category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _categoryService.DeleteAsync(id, userId);
        return NoContent();
    }
}
