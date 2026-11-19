using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;

namespace TestTakingService.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetCurrentUserUid()
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subClaim, out var userUid))
        {
            throw new UnauthorizedException("Некорректный идентификатор пользователя в токене.");
        }

        return userUid;
    }
}