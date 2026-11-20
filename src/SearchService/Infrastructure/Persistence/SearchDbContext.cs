using Microsoft.EntityFrameworkCore;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Persistence;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    public DbSet<SearchIndex> SearchIndices => Set<SearchIndex>();
    public DbSet<TestStatistics> TestStatistics => Set<TestStatistics>();
    public DbSet<ProcessedAttempt> ProcessedAttempts => Set<ProcessedAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("search_schema");

        // 1. search_index
        modelBuilder.Entity<SearchIndex>(entity =>
        {
            entity.ToTable("search_index");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.TestUid)
                .HasColumnName("test_uid")
                .IsRequired();
            entity.HasIndex(e => e.TestUid).IsUnique();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.AuthorUid)
                .HasColumnName("author_uid")
                .IsRequired();
            entity.HasIndex(e => e.AuthorUid);

            entity.Property(e => e.AuthorName)
                .HasColumnName("author_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Subject)
                .HasColumnName("subject")
                .HasMaxLength(100);
            entity.HasIndex(e => e.Subject);

            entity.Property(e => e.Tags)
                .HasColumnName("tags")
                .HasMaxLength(500);

            entity.Property(e => e.IsPublic)
                .HasColumnName("is_public")
                .HasDefaultValue(true)
                .IsRequired();
            entity.HasIndex(e => e.IsPublic);

            entity.Property(e => e.QuestionCount)
                .HasColumnName("question_count")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.SearchVector)
                .HasColumnName("search_vector")
                .HasComputedColumnSql(
                    "setweight(to_tsvector('russian', coalesce(title, '')), 'A') || " +
                    "setweight(to_tsvector('russian', coalesce(description, '')), 'B') || " +
                    "setweight(to_tsvector('russian', coalesce(tags, '')), 'B') || " +
                    "setweight(to_tsvector('russian', coalesce(subject, '')), 'C') || " +
                    "setweight(to_tsvector('russian', coalesce(author_name, '')), 'C')",
                    stored: true);

            entity.HasIndex(e => e.SearchVector)
                .HasMethod("GIN");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();
        });

        // 2. test_statistics
        modelBuilder.Entity<TestStatistics>(entity =>
        {
            entity.ToTable("test_statistics");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.TestUid)
                .HasColumnName("test_uid")
                .IsRequired();
            entity.HasIndex(e => e.TestUid).IsUnique();

            entity.Property(e => e.TotalAttempts)
                .HasColumnName("total_attempts")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.PassedAttempts)
                .HasColumnName("passed_attempts")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.PassRate)
                .HasColumnName("pass_rate")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            entity.Property(e => e.AvgScore)
                .HasColumnName("avg_score")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            entity.Property(e => e.AvgDurationSec)
                .HasColumnName("avg_duration_sec")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.LastAttemptAt)
                .HasColumnName("last_attempt_at");
        });

        // 3. processed_attempts
        modelBuilder.Entity<ProcessedAttempt>(entity =>
        {
            entity.ToTable("processed_attempts");

            entity.HasKey(e => e.AttemptUid);
            entity.Property(e => e.AttemptUid)
                .HasColumnName("attempt_uid")
                .ValueGeneratedNever();

            entity.Property(e => e.ProcessedAt)
                .HasColumnName("processed_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();
        });
    }
}