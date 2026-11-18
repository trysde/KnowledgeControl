using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using UserService.Contracts.Bans;
using UserService.Data;
using UserService.Domain.Entities;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class BanService(UserDbContext dbContext) : IBanService
{
    public async Task BanAsync(Guid moderatorUid, BanRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserUid == moderatorUid)
        {
            throw new BadRequestException("Модератор не может заблокировать самого себя.");
        }

        var moderator = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Uid == moderatorUid, cancellationToken)
            ?? throw new UnauthorizedException("Модератор не найден.");

        var targetUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Uid == request.UserUid, cancellationToken)
            ?? throw new NotFoundException($"Пользователь с Uid {request.UserUid} не найден.");

        if (targetUser.IsModerator)
        {
            throw new ForbiddenException("Нельзя заблокировать другого модератора.");
        }

        if (await IsBannedAsync(targetUser.Id, cancellationToken))
        {
            throw new ConflictException("Пользователь уже заблокирован.");
        }

        var ban = new Ban
        {
            UserId = targetUser.Id,
            ModeratorId = moderator.Id,
            Reason = request.Reason.Trim(),
            BannedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt?.UtcDateTime
        };

        dbContext.Bans.Add(ban);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnbanAsync(Guid targetUserUid, Guid moderatorUid, CancellationToken cancellationToken = default)
    {
        var targetUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Uid == targetUserUid, cancellationToken)
            ?? throw new NotFoundException($"Пользователь с Uid {targetUserUid} не найден.");

        var now = DateTime.UtcNow;

        var activeBans = await dbContext.Bans
            .Where(b => b.UserId == targetUser.Id
                     && b.UnbannedAt == null
                     && (b.ExpiresAt == null || b.ExpiresAt > now))
            .ToListAsync(cancellationToken);

        if (activeBans.Count == 0)
        {
            throw new NotFoundException("У пользователя нет активных блокировок.");
        }

        foreach (var ban in activeBans)
        {
            ban.UnbannedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsBannedAsync(long userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await dbContext.Bans
            .AnyAsync(b => b.UserId == userId
                        && b.UnbannedAt == null
                        && (b.ExpiresAt == null || b.ExpiresAt > now),
                      cancellationToken);
    }
}