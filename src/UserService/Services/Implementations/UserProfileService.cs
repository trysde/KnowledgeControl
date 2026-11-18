using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using UserService.Contracts.Users;
using UserService.Data;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class UserProfileService(UserDbContext dbContext) : IUserService
{
    public async Task<UserInfoResponse> GetMyInfoAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Uid == userUid, cancellationToken)
            ?? throw new NotFoundException("Пользователь не найден.");

        return new UserInfoResponse(
            Uid: user.Uid,
            Nickname: user.Nickname,
            Email: user.Email,
            IsModerator: user.IsModerator,
            CreatedAt: new DateTimeOffset(user.CreatedAt, TimeSpan.Zero));
    }

    public async Task<UserInfoResponse> UpdateNicknameAsync(Guid userUid, UpdateNicknameRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Uid == userUid, cancellationToken)
            ?? throw new NotFoundException("Пользователь не найден.");

        var trimmedNickname = request.NewNickname.Trim();

        var nicknameTaken = await dbContext.Users
            .AnyAsync(u => u.Uid != userUid && u.Nickname.ToLower() == trimmedNickname.ToLower(), cancellationToken);

        if (nicknameTaken)
        {
            throw new ConflictException($"Никнейм '{trimmedNickname}' уже занят.");
        }

        user.Nickname = trimmedNickname;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserInfoResponse(
            Uid: user.Uid,
            Nickname: user.Nickname,
            Email: user.Email,
            IsModerator: user.IsModerator,
            CreatedAt: new DateTimeOffset(user.CreatedAt, TimeSpan.Zero));
    }
}