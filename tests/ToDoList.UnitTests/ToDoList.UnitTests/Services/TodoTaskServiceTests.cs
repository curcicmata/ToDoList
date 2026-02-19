using FluentAssertions;
using Moq;
using ToDoList.Application.DTOs;
using ToDoList.Application.Services;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;

namespace ToDoList.UnitTests.Services;

public class TodoTaskServiceTests
{
    private readonly Mock<ITodoTaskRepository> _mockTaskRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly TodoTaskService _service;
    private readonly Guid _testUserId = Guid.NewGuid();

    public TodoTaskServiceTests()
    {
        _mockTaskRepository = new Mock<ITodoTaskRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _service = new TodoTaskService(_mockTaskRepository.Object, _mockCategoryRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTodoTaskDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            CategoryId = categoryId,
            Category = new Category { Id = categoryId, Name = "Work" },
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync(task);

        // Act
        var result = await _service.GetByIdAsync(taskId, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.InProgress);
        result.Priority.Should().Be(TaskPriority.High);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be("Work");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync((TodoTask?)null);

        // Act
        var result = await _service.GetByIdAsync(taskId, _testUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Status = ToDoList.Domain.Entities.TaskStatus.Completed,
                Priority = TaskPriority.Low,
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockTaskRepository
            .Setup(r => r.GetAllByUserIdAsync(_testUserId, null, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllByUserIdAsync(_testUserId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Title.Should().Be("Task 1");
        resultList[1].Title.Should().Be("Task 2");
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithStatusFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Completed Task",
                Status = ToDoList.Domain.Entities.TaskStatus.Completed,
                Priority = TaskPriority.Medium,
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockTaskRepository
            .Setup(r => r.GetAllByUserIdAsync(_testUserId, ToDoList.Domain.Entities.TaskStatus.Completed, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllByUserIdAsync(_testUserId, ToDoList.Domain.Entities.TaskStatus.Completed);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList[0].Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.Completed);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = ToDoList.Domain.Entities.TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockTaskRepository
            .Setup(r => r.GetPagedAsync(_testUserId, 1, 10, null, null, null, false))
            .ReturnsAsync((tasks, 25));

        // Act
        var result = await _service.GetPagedAsync(_testUserId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task CreateAsync_WithoutCategory_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var createDto = new CreateTodoTaskDto
        {
            Title = "New Task",
            Description = "Task Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        TodoTask? capturedTask = null;
        _mockTaskRepository
            .Setup(r => r.CreateAsync(It.IsAny<TodoTask>()))
            .Callback<TodoTask>(t => capturedTask = t)
            .ReturnsAsync((TodoTask t) => t);

        // Act
        var result = await _service.CreateAsync(createDto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.Description.Should().Be("Task Description");
        result.Priority.Should().Be(TaskPriority.High);
        result.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.Pending);

        capturedTask.Should().NotBeNull();
        capturedTask!.Id.Should().NotBeEmpty();
        capturedTask.UserId.Should().Be(_testUserId);
        capturedTask.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.Pending);
        capturedTask.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateAsync_WithValidCategory_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createDto = new CreateTodoTaskDto
        {
            Title = "New Task",
            Description = "Task Description",
            Priority = TaskPriority.Medium,
            CategoryId = categoryId
        };

        _mockCategoryRepository
            .Setup(r => r.ExistsAsync(categoryId, _testUserId))
            .ReturnsAsync(true);

        _mockTaskRepository
            .Setup(r => r.CreateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync((TodoTask t) => t);

        // Act
        var result = await _service.CreateAsync(createDto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCategory_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createDto = new CreateTodoTaskDto
        {
            Title = "New Task",
            CategoryId = categoryId
        };

        _mockCategoryRepository
            .Setup(r => r.ExistsAsync(categoryId, _testUserId))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(createDto, _testUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TodoTask
        {
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description",
            Status = ToDoList.Domain.Entities.TaskStatus.Pending,
            Priority = TaskPriority.Low,
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync(existingTask);

        _mockTaskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync((TodoTask t) => t);

        // Act
        var result = await _service.UpdateAsync(taskId, updateDto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.InProgress);
        result.Priority.Should().Be(TaskPriority.High);

        existingTask.UpdatedAt.Should().NotBeNull();
        existingTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingStatusToCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TodoTask
        {
            Id = taskId,
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.Completed,
            Priority = TaskPriority.Medium
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync(existingTask);

        _mockTaskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync((TodoTask t) => t);

        // Act
        var result = await _service.UpdateAsync(taskId, updateDto, _testUserId);

        // Assert
        result.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.Completed);
        existingTask.CompletedAt.Should().NotBeNull();
        existingTask.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingStatusFromCompleted_ShouldClearCompletedAt()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TodoTask
        {
            Id = taskId,
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.Completed,
            Priority = TaskPriority.Medium,
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync(existingTask);

        _mockTaskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync((TodoTask t) => t);

        // Act
        var result = await _service.UpdateAsync(taskId, updateDto, _testUserId);

        // Assert
        result.Status.Should().Be(ToDoList.Domain.Entities.TaskStatus.InProgress);
        existingTask.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Updated Title",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync((TodoTask?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(taskId, updateDto, _testUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Task with ID {taskId} not found");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidCategory_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingTask = new TodoTask
        {
            Id = taskId,
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.Pending,
            Priority = TaskPriority.Medium,
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow
        };

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Task",
            Status = ToDoList.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CategoryId = categoryId
        };

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, _testUserId))
            .ReturnsAsync(existingTask);

        _mockCategoryRepository
            .Setup(r => r.ExistsAsync(categoryId, _testUserId))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(taskId, updateDto, _testUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        await _service.DeleteAsync(taskId, _testUserId);

        // Assert
        _mockTaskRepository.Verify(
            r => r.DeleteAsync(taskId, _testUserId),
            Times.Once);
    }

    [Fact]
    public async Task GetOverdueCountAsync_ShouldReturnCountFromRepository()
    {
        // Arrange
        _mockTaskRepository
            .Setup(r => r.GetOverdueCountAsync(_testUserId))
            .ReturnsAsync(5);

        // Act
        var result = await _service.GetOverdueCountAsync(_testUserId);

        // Assert
        result.Should().Be(5);
    }
}
