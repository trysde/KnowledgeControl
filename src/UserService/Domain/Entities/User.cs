namespace UserService.Domain.Entities;

public class User
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Nickname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsModerator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public ICollection<Ban> Bans { get; set; } = new List<Ban>();
    public ICollection<Ban> IssuedBans { get; set; } = new List<Ban>();
}
