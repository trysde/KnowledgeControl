using Microsoft.EntityFrameworkCore;
using TestTakingService.Domain.Entities;

namespace TestTakingService.Infrastructure.Persistence;

public class TestTakingDbContext(DbContextOptions<TestTakingDbContext> options) : DbContext(options)
{
    public const string SchemaName = "test_taking_schema";

    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestTakingDbContext).Assembly);
    }
}