using UserService.Contracts.Auth;
using UserService.Contracts.Users;

namespace UserService.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userUid, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}