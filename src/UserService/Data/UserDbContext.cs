using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ban> Bans => Set<Ban>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("user_schema");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");

            entity.Property(u => u.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(u => u.Uid).IsUnique();

            entity.Property(u => u.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(64)
                .IsRequired();
            entity.HasIndex(u => u.Nickname).IsUnique();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.IsModerator)
                .HasColumnName("is_moderator")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.ToTable("bans");

            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).HasColumnName("id");

            entity.Property(b => b.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(b => b.ModeratorId)
                .HasColumnName("moderator_id")
                .IsRequired();

            entity.Property(b => b.Reason)
                .HasColumnName("reason")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(b => b.BannedAt)
                .HasColumnName("banned_at")
                .IsRequired();

            entity.Property(b => b.ExpiresAt)
                .HasColumnName("expires_at");

            entity.Property(b => b.UnbannedAt)
                .HasColumnName("unbanned_at");

            // Индекс для быстрой проверки активных банов пользователя
            entity.HasIndex(b => new { b.UserId, b.UnbannedAt });

            entity.HasOne(b => b.User)
                .WithMany(u => u.Bans)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Moderator)
                .WithMany(u => u.IssuedBans)
                .HasForeignKey(b => b.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}