namespace TestTakingService.Domain.Entities;

public class Result
{
    public long Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public long AttemptId { get; set; }
    public Guid TestUid { get; set; }
    public Guid UserUid { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool Passed { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public Attempt Attempt { get; set; } = null!;
}