namespace UserService.Contracts.Auth;

public record LoginResponse(Guid Uid, string Token);