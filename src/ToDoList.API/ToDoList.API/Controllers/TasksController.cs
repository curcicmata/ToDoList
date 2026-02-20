using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;

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
    /// Returns all tasks without pagination for the authenticated user
    /// </summary>
    /// <param name="status">Optional filter by task status (Pending=0, InProgress=1, Completed=2, Cancelled=3)</param>
    /// <param name="categoryId">Optional filter by category ID</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TodoTaskDto>>> GetAll(
        [FromQuery] ToDoList.Domain.Entities.TaskStatus? status = null,
        [FromQuery] Guid? categoryId = null)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllByUserIdAsync(userId, status, categoryId);
        return Ok(tasks);
    }

    /// <summary>
    /// Get paginated tasks for the authenticated user with filtering and sorting
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="status">Optional filter by task status (Pending=0, InProgress=1, Completed=2, Cancelled=3)</param>
    /// <param name="categoryId">Optional filter by category ID</param>
    /// <param name="sortBy">Optional sort field (Title, DueDate, Priority, CreatedAt)</param>
    /// <param name="sortDescending">Sort in descending order (default: false)</param>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResultDto<TodoTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PagedResultDto<TodoTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpGet("overdue/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetOverdueCount()
    {
        var userId = GetUserId();
        var count = await _taskService.GetOverdueCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="dto">Task creation details including title, description, priority, due date, and category</param>
    [HttpPost]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID to update</param>
    /// <param name="dto">Updated task details including title, description, status, priority, due date, and category</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _taskService.DeleteAsync(id, userId);
        return NoContent();
    }
}
