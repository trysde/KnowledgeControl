using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestTakingService.Domain.Entities;

namespace TestTakingService.Infrastructure.Persistence.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        builder.Property(a => a.QuestionUid)
            .HasColumnName("question_uid")
            .IsRequired();

        builder.Property(a => a.SelectedOptionUids)
            .HasColumnName("selected_option_uids")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<Guid>>(v, JsonOptions) ?? new List<Guid>(),
                new ValueComparer<List<Guid>>(
                    (l, r) => (l ?? new()).SequenceEqual(r ?? new()),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    v => v.ToList()))
            .IsRequired();

        builder.Property(a => a.TextAnswer)
            .HasColumnName("text_answer")
            .HasMaxLength(500);

        builder.Property(a => a.IsCorrect)
            .HasColumnName("is_correct");

        builder.Property(a => a.PointsEarned)
            .HasColumnName("points_earned")
            .HasPrecision(6, 2);

        builder.Property(a => a.AnsweredAt)
            .HasColumnName("answered_at")
            .IsRequired();

        // Ограничение UNIQUE(attempt_id, question_uid) для идемпотентного upsert ответа
        builder.HasIndex(a => new { a.AttemptId, a.QuestionUid })
            .IsUnique();
    }
}