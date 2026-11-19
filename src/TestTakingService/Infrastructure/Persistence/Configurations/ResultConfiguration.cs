using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTakingService.Domain.Entities;

namespace TestTakingService.Infrastructure.Persistence.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("results");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Uid)
            .HasColumnName("uid")
            .IsRequired();
        builder.HasIndex(r => r.Uid).IsUnique();

        builder.Property(r => r.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        builder.Property(r => r.TestUid)
            .HasColumnName("test_uid")
            .IsRequired();

        builder.Property(r => r.UserUid)
            .HasColumnName("user_uid")
            .IsRequired();

        builder.Property(r => r.TotalScore)
            .HasColumnName("total_score")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(r => r.MaxScore)
            .HasColumnName("max_score")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(r => r.Percentage)
            .HasColumnName("percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.Passed)
            .HasColumnName("passed")
            .IsRequired();

        builder.Property(r => r.CalculatedAt)
            .HasColumnName("calculated_at")
            .IsRequired();

        // Связь 1:1 (FK + UNIQUE на attempt_id)
        builder.HasIndex(r => r.AttemptId)
            .IsUnique();

        builder.HasIndex(r => new { r.UserUid, r.TestUid });
    }
}