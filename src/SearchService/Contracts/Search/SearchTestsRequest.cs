namespace SearchService.Contracts.Search;

public record SearchTestsRequest
{
    public string? Q { get; init; }
    public string? Category { get; init; }
    public string? Tag { get; init; }
    public Guid? Author { get; init; }
    public string Sort { get; init; } = "relevance";
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
}