namespace KnowledgeControl.UserService.Contracts.Users;

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}