namespace KnowledgeControl.UserService.Contracts.Users;

public record UserInfoResponse(
    Guid Uid,
    string Nickname,
    string Email,
    bool IsModerator,
    DateTimeOffset CreatedAt);