using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Contracts.Users;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[Route("api/users")]
[Authorize]
public class UserController(IUserService userService) : BaseApiController
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoResponse>> GetMyInfo(CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await userService.GetMyInfoAsync(userUid, cancellationToken);
        return Ok(response);
    }

    [HttpPut("me/nickname")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserInfoResponse>> UpdateNickname(
        [FromBody] UpdateNicknameRequest request,
        CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await userService.UpdateNicknameAsync(userUid, request, cancellationToken);
        return Ok(response);
    }
}