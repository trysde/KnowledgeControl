namespace UserService.Contracts.Bans;

public record BanRequest
{
    public required Guid UserUid { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}