using ToDoList.Domain.Entities;

namespace ToDoList.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
