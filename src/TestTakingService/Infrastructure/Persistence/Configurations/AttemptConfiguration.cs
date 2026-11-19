using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTakingService.Domain.Entities;
using TestTakingService.Domain.Enums;
using TestTakingService.Domain.ValueObjects;

namespace TestTakingService.Infrastructure.Persistence.Configurations;

public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Uid)
            .HasColumnName("uid")
            .IsRequired();
        builder.HasIndex(a => a.Uid).IsUnique();

        builder.Property(a => a.TestUid)
            .HasColumnName("test_uid")
            .IsRequired();

        builder.Property(a => a.UserUid)
            .HasColumnName("user_uid")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion(
                v => ToSnakeUpper(v),
                v => FromSnakeUpper(v))
            .HasMaxLength(20)
            .HasDefaultValue(AttemptStatus.InProgress)
            .IsRequired();

        builder.Property(a => a.Snapshot)
            .HasColumnName("snapshot")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<AttemptSnapshot>(v, JsonOptions) ?? new AttemptSnapshot(),
                new ValueComparer<AttemptSnapshot>(
                    (l, r) => JsonSerializer.Serialize(l, JsonOptions) == JsonSerializer.Serialize(r, JsonOptions),
                    v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<AttemptSnapshot>(
                        JsonSerializer.Serialize(v, JsonOptions), JsonOptions) ?? new AttemptSnapshot()))
            .IsRequired();

        builder.Property(a => a.SnapshotVersion)
            .HasColumnName("snapshot_version")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(a => a.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(a => a.FinishedAt)
            .HasColumnName("finished_at");

        builder.Property(a => a.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(a => new { a.UserUid, a.TestUid });
        builder.HasIndex(a => a.Status);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.Attempt)
            .HasForeignKey(ans => ans.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Result)
            .WithOne(r => r.Attempt)
            .HasForeignKey<Result>(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string ToSnakeUpper(AttemptStatus status) => status switch
    {
        AttemptStatus.InProgress => "IN_PROGRESS",
        AttemptStatus.Completed => "COMPLETED",
        AttemptStatus.Expired => "EXPIRED",
        _ => status.ToString().ToUpperInvariant()
    };

    private static AttemptStatus FromSnakeUpper(string value) => value switch
    {
        "IN_PROGRESS" => AttemptStatus.InProgress,
        "COMPLETED" => AttemptStatus.Completed,
        "EXPIRED" => AttemptStatus.Expired,
        _ => Enum.Parse<AttemptStatus>(value, ignoreCase: true)
    };
}