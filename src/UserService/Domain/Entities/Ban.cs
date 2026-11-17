namespace UserService.Domain.Entities;

public class Ban
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ModeratorId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? UnbannedAt { get; set; }

    // Навигационные свойства
    public User User { get; set; } = null!;
    public User Moderator { get; set; } = null!;
}