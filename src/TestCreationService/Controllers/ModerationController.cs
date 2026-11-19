using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestCreationService.Contracts.Moderation;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Controllers;

[Route("api/moderation")]
[Authorize]
public class ModerationController(IModerationService moderationService) : BaseApiController
{
    [HttpPost("report")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReportResponse>> Report(
        [FromBody] ReportRequest request,
        CancellationToken cancellationToken)
    {
        var reporterUid = GetCurrentUserUid();
        var response = await moderationService.ReportAsync(reporterUid, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("hide/{testUid:guid}")]
    [Authorize(Policy = "RequireModerator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Hide(
        [FromRoute] Guid testUid,
        [FromQuery] string? comment,
        CancellationToken cancellationToken)
    {
        var moderatorUid = GetCurrentUserUid();
        await moderationService.HideAsync(testUid, moderatorUid, comment, cancellationToken);
        return NoContent();
    }
}