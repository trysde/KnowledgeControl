using UserService.Domain.Entities;

namespace UserService.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}