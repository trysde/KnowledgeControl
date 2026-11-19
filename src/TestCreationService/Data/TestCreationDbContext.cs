using Microsoft.EntityFrameworkCore;
using TestCreationService.Domain.Entities;
using TestCreationService.Domain.Enums;

namespace TestCreationService.Data;

public class TestCreationDbContext : DbContext
{
    public TestCreationDbContext(DbContextOptions<TestCreationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();
    public DbSet<ModerationReport> ModerationReports => Set<ModerationReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("test_creation_schema");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");

            entity.Property(c => c.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(c => c.Uid).IsUnique();

            entity.Property(c => c.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();

            entity.Property(c => c.Description)
                .HasColumnName("description")
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");

            entity.Property(t => t.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(t => t.Uid).IsUnique();

            entity.Property(t => t.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("questions");

            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName("id");

            entity.Property(q => q.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(q => q.Uid).IsUnique();

            entity.Property(q => q.AuthorUid)
                .HasColumnName("author_uid")
                .IsRequired();
            entity.HasIndex(q => q.AuthorUid);

            entity.Property(q => q.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(q => q.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(q => q.Text)
                .HasColumnName("text")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(q => q.QuestionType)
                .HasColumnName("question_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(q => q.CorrectTextAnswer)
                .HasColumnName("correct_text_answer")
                .HasMaxLength(500);

            entity.Property(q => q.IsPublic)
                .HasColumnName("is_public")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(q => q.Version)
                .HasColumnName("version")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(q => q.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(q => q.IsBlocked)
                .HasColumnName("is_blocked")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(q => q.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(q => q.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasIndex(q => new { q.CategoryId, q.IsPublic, q.IsDeleted, q.IsBlocked });

            entity.HasOne(q => q.Category)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.ToTable("answer_options");

            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");

            entity.Property(a => a.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(a => a.Uid).IsUnique();

            entity.Property(a => a.QuestionId)
                .HasColumnName("question_id")
                .IsRequired();

            entity.Property(a => a.Text)
                .HasColumnName("text")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(a => a.IsCorrect)
                .HasColumnName("is_correct")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(a => a.Position)
                .HasColumnName("position")
                .IsRequired();

            entity.HasIndex(a => new { a.QuestionId, a.Position });

            entity.HasOne(a => a.Question)
                .WithMany(q => q.AnswerOptions)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionTag>(entity =>
        {
            entity.ToTable("question_tags");

            entity.HasKey(qt => new { qt.QuestionId, qt.TagId });

            entity.Property(qt => qt.QuestionId).HasColumnName("question_id");
            entity.Property(qt => qt.TagId).HasColumnName("tag_id");

            entity.HasOne(qt => qt.Question)
                .WithMany(q => q.QuestionTags)
                .HasForeignKey(qt => qt.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(qt => qt.Tag)
                .WithMany(t => t.QuestionTags)
                .HasForeignKey(qt => qt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.ToTable("tests");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");

            entity.Property(t => t.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(t => t.Uid).IsUnique();

            entity.Property(t => t.AuthorUid)
                .HasColumnName("author_uid")
                .IsRequired();
            entity.HasIndex(t => t.AuthorUid);

            entity.Property(t => t.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(t => t.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            entity.Property(t => t.TimeLimitSeconds)
                .HasColumnName("time_limit_seconds");

            entity.Property(t => t.MaxAttempts)
                .HasColumnName("max_attempts");

            entity.Property(t => t.PassingScore)
                .HasColumnName("passing_score")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(t => t.StartsAt)
                .HasColumnName("starts_at");

            entity.Property(t => t.EndsAt)
                .HasColumnName("ends_at");

            entity.Property(t => t.IsPublic)
                .HasColumnName("is_public")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(t => t.IsHidden)
                .HasColumnName("is_hidden")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(t => t.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(TestStatus.Draft)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasIndex(t => new { t.CategoryId, t.Status, t.IsPublic, t.IsHidden });

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Tests)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TestQuestion>(entity =>
        {
            entity.ToTable("test_questions");

            entity.HasKey(tq => new { tq.TestId, tq.QuestionId });

            entity.Property(tq => tq.TestId).HasColumnName("test_id");
            entity.Property(tq => tq.QuestionId).HasColumnName("question_id");

            entity.Property(tq => tq.OrderIndex)
                .HasColumnName("order_index")
                .IsRequired();

            entity.Property(tq => tq.Points)
                .HasColumnName("points")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(tq => tq.Snapshot)
                .HasColumnName("snapshot")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.HasIndex(tq => new { tq.TestId, tq.OrderIndex });

            entity.HasOne(tq => tq.Test)
                .WithMany(t => t.TestQuestions)
                .HasForeignKey(tq => tq.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tq => tq.Question)
                .WithMany(q => q.TestQuestions)
                .HasForeignKey(tq => tq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModerationReport>(entity =>
        {
            entity.ToTable("moderation_reports");

            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id");

            entity.Property(r => r.Uid)
                .HasColumnName("uid")
                .IsRequired();
            entity.HasIndex(r => r.Uid).IsUnique();

            entity.Property(r => r.ReporterUid)
                .HasColumnName("reporter_uid")
                .IsRequired();

            entity.Property(r => r.TargetType)
                .HasColumnName("target_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(r => r.TargetUid)
                .HasColumnName("target_uid")
                .IsRequired();

            entity.Property(r => r.Reason)
                .HasColumnName("reason")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(ModerationReportStatus.Pending)
                .IsRequired();

            entity.Property(r => r.ModeratorUid)
                .HasColumnName("moderator_uid");

            entity.Property(r => r.ModeratorComment)
                .HasColumnName("moderator_comment")
                .HasMaxLength(1000);

            entity.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(r => r.ResolvedAt)
                .HasColumnName("resolved_at");

            entity.HasIndex(r => new { r.Status, r.CreatedAt });
            entity.HasIndex(r => new { r.TargetType, r.TargetUid });
        });

        base.OnModelCreating(modelBuilder);
    }
}