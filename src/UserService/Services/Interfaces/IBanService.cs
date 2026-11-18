using UserService.Contracts.Bans;

namespace UserService.Services.Interfaces;

public interface IBanService
{
    Task BanAsync(Guid moderatorUid, BanRequest request, CancellationToken cancellationToken = default);
    Task UnbanAsync(Guid targetUserUid, Guid moderatorUid, CancellationToken cancellationToken = default);
    Task<bool> IsBannedAsync(long userId, CancellationToken cancellationToken = default);
}