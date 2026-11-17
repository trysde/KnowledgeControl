namespace KnowledgeControl.UserService.Contracts.Auth;

public record LoginResponse(Guid Uid, string Token);