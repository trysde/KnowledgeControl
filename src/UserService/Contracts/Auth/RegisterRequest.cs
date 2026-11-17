namespace KnowledgeControl.UserService.Contracts.Auth;

public record RegisterRequest
{
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}