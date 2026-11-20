namespace SearchService.Domain.Entities;

public class TestStatistics
{
    public int Id { get; set; }
    public Guid TestUid { get; set; }
    public int TotalAttempts { get; set; }
    public int PassedAttempts { get; set; }
    public decimal PassRate { get; set; }
    public decimal AvgScore { get; set; }
    public int AvgDurationSec { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}