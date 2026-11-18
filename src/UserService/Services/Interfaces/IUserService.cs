using UserService.Contracts.Users;

namespace UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserInfoResponse> GetMyInfoAsync(Guid userUid, CancellationToken cancellationToken = default);
    Task<UserInfoResponse> UpdateNicknameAsync(Guid userUid, UpdateNicknameRequest request, CancellationToken cancellationToken = default);
}