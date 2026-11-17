namespace KnowledgeControl.UserService.Contracts.Users;

public record UpdateNicknameRequest
{
    public required string NewNickname { get; init; }
}