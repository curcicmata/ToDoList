using FluentAssertions;
using Moq;
using ToDoList.Application.DTOs;
using ToDoList.Application.Services;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;

namespace ToDoList.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly CategoryService _service;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CategoryServiceTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _service = new CategoryService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategoryDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Work",
            Description = "Work related tasks",
            Color = "#FF5733",
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow,
            Tasks = new List<TodoTask>
            {
                new() { Id = Guid.NewGuid() },
                new() { Id = Guid.NewGuid() }
            }
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(categoryId, _testUserId))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetByIdAsync(categoryId, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Work");
        result.Description.Should().Be("Work related tasks");
        result.Color.Should().Be("#FF5733");
        result.TaskCount.Should().Be(2);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(categoryId, _testUserId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.GetByIdAsync(categoryId, _testUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Work",
                Description = "Work tasks",
                Color = "#FF5733",
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow,
                Tasks = new List<TodoTask> { new TodoTask() }
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Personal",
                Description = "Personal tasks",
                Color = "#33FF57",
                UserId = _testUserId,
                CreatedAt = DateTime.UtcNow,
                Tasks = new List<TodoTask>()
            }
        };

        _mockRepository
            .Setup(r => r.GetAllByUserIdAsync(_testUserId))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllByUserIdAsync(_testUserId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Name.Should().Be("Work");
        resultList[0].TaskCount.Should().Be(1);
        resultList[1].Name.Should().Be("Personal");
        resultList[1].TaskCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategoryAndReturnDto()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Shopping",
            Description = "Shopping list",
            Color = "#5733FF"
        };

        Category? capturedCategory = null;
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .Callback<Category>(c => capturedCategory = c)
            .ReturnsAsync((Category c) => c);

        // Act
        var result = await _service.CreateAsync(createDto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Shopping");
        result.Description.Should().Be("Shopping list");
        result.Color.Should().Be("#5733FF");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        capturedCategory.Should().NotBeNull();
        capturedCategory!.Id.Should().NotBeEmpty();
        capturedCategory.UserId.Should().Be(_testUserId);
        capturedCategory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            Description = "Old Description",
            Color = "#000000",
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "#FFFFFF"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(categoryId, _testUserId))
            .ReturnsAsync(existingCategory);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Color.Should().Be("#FFFFFF");

        existingCategory.Name.Should().Be("Updated Name");
        existingCategory.Description.Should().Be("Updated Description");
        existingCategory.Color.Should().Be("#FFFFFF");
        existingCategory.UpdatedAt.Should().NotBeNull();
        existingCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "#FFFFFF"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(categoryId, _testUserId))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(categoryId, updateDto, _testUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        await _service.DeleteAsync(categoryId, _testUserId);

        // Assert
        _mockRepository.Verify(
            r => r.DeleteAsync(categoryId, _testUserId),
            Times.Once);
    }
}
