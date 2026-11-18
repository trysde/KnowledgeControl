using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Contracts.Bans;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[Route("api/bans")]
[Authorize(Policy = "RequireModerator")]
public class BanController(IBanService banService) : BaseApiController
{
    [HttpPost("ban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ban(
        [FromBody] BanRequest request,
        CancellationToken cancellationToken)
    {
        var moderatorUid = GetCurrentUserUid();
        await banService.BanAsync(moderatorUid, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("unban/{userUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unban(
        [FromRoute] Guid userUid,
        CancellationToken cancellationToken)
    {
        var moderatorUid = GetCurrentUserUid();
        await banService.UnbanAsync(userUid, moderatorUid, cancellationToken);
        return NoContent();
    }
}