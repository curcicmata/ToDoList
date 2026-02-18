using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;
using ToDoList.Domain.Entities;

namespace ToDoList.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController(ITodoTaskService taskService) : ControllerBase
{
    private readonly ITodoTaskService _taskService = taskService;

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Get all tasks from a user
    /// </summary>
    /// <param name="status"></param>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoTaskDto>>> GetAll(
        [FromQuery] ToDoList.Domain.Entities.TaskStatus? status = null,
        [FromQuery] Guid? categoryId = null)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllByUserIdAsync(userId, status, categoryId);
        return Ok(tasks);
    }

    /// <summary>
    /// Get tasks from a user that have pagination
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="status"></param>
    /// <param name="categoryId"></param>
    /// <param name="sortBy"></param>
    /// <param name="sortDescending"></param>
    /// <returns></returns>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<TodoTaskDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ToDoList.Domain.Entities.TaskStatus? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var userId = GetUserId();
        var result = await _taskService.GetPagedAsync(userId, pageNumber, pageSize, status, categoryId, sortBy, sortDescending);
        return Ok(result);
    }

    /// <summary>
    /// Get single user task
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoTaskDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var task = await _taskService.GetByIdAsync(id, userId);

        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(task);
    }

    /// <summary>
    /// Count all overdue taks from logged user
    /// </summary>
    /// <returns></returns>
    [HttpGet("overdue/count")]
    public async Task<ActionResult<int>> GetOverdueCount()
    {
        var userId = GetUserId();
        var count = await _taskService.GetOverdueCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>
    /// Create a task
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<TodoTaskDto>> Create([FromBody] CreateTodoTaskDto dto)
    {
        try
        {
            var userId = GetUserId();
            var task = await _taskService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a task
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoTaskDto>> Update(Guid id, [FromBody] UpdateTodoTaskDto dto)
    {
        try
        {
            var userId = GetUserId();
            var task = await _taskService.UpdateAsync(id, dto, userId);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _taskService.DeleteAsync(id, userId);
        return NoContent();
    }
}
