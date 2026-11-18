using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;
using UserService.Contracts.Auth;
using UserService.Contracts.Users;
using UserService.Data;
using UserService.Domain.Entities;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class AuthService(
    UserDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IBanService banService) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var trimmedNickname = request.Nickname.Trim();

        var emailExists = await dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException($"Email '{request.Email}' уже используется.");
        }

        var nicknameExists = await dbContext.Users
            .AnyAsync(u => u.Nickname.ToLower() == trimmedNickname.ToLower(), cancellationToken);

        if (nicknameExists)
        {
            throw new ConflictException($"Никнейм '{trimmedNickname}' уже занят.");
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            Uid = Guid.NewGuid(),
            Email = normalizedEmail,
            Nickname = trimmedNickname,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            IsModerator = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.GenerateToken(user);

        return new RegisterResponse(user.Uid, token);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Неверный email или пароль.");
        }

        if (await banService.IsBannedAsync(user.Id, cancellationToken))
        {
            throw new ForbiddenException("Ваш аккаунт заблокирован.");
        }

        var token = jwtTokenService.GenerateToken(user);

        return new LoginResponse(user.Uid, token);
    }

    public async Task ChangePasswordAsync(Guid userUid, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Uid == userUid, cancellationToken)
            ?? throw new NotFoundException("Пользователь не найден.");

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Текущий пароль указан неверно.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}