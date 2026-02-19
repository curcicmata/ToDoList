using FluentAssertions;
using Moq;
using ToDoList.Application.DTOs;
using ToDoList.Application.Interfaces;
using ToDoList.Application.Services;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Enums;
using ToDoList.Domain.Interfaces;

namespace ToDoList.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _service = new AuthService(_mockUserRepository.Object, _mockJwtTokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserRepository
            .Setup(r => r.EmailExistsAsync(registerDto.Email))
            .ReturnsAsync(false);

        User? capturedUser = null;
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        // Act
        var result = await _service.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be("User");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));

        capturedUser.Should().NotBeNull();
        capturedUser!.Id.Should().NotBeEmpty();
        capturedUser.Email.Should().Be("test@example.com");
        capturedUser.Role.Should().Be(UserRole.User);
        capturedUser.IsDeleted.Should().BeFalse();
        capturedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify password was hashed (should not be plain text)
        capturedUser.PasswordHash.Should().NotBe("password123");
        capturedUser.PasswordHash.Should().NotBeNullOrEmpty();

        // Verify password hash is valid BCrypt hash (starts with $2)
        capturedUser.PasswordHash.Should().StartWith("$2");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserRepository
            .Setup(r => r.EmailExistsAsync(registerDto.Email))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email already registered");

        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(user))
            .Returns("test-jwt-token");

        // Act
        var result = await _service.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be("User");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _service.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = UserRole.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _service.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.User,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _service.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Role = UserRole.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be("User");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Role = UserRole.User,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}
