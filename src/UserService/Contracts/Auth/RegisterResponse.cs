namespace UserService.Contracts.Auth;

public record RegisterResponse(Guid Uid, string Token);