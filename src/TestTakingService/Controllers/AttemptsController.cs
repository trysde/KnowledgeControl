using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using TestTakingService.Contracts.Attempts;
using TestTakingService.Services.Interfaces;

namespace TestTakingService.Controllers;

[Route("api/attempts")]
[Authorize]
public class AttemptsController(IAttemptService attemptService) : BaseApiController
{
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartAttemptResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StartAttemptResponse>> Start(
        [FromQuery] Guid testUid,
        CancellationToken cancellationToken)
    {
        if (testUid == Guid.Empty)
        {
            throw new BadRequestException("Параметр запроса 'testUid' обязателен и не может быть пустым.");
        }

        var userUid = GetCurrentUserUid();
        var response = await attemptService.StartAsync(testUid, userUid, cancellationToken);
        return CreatedAtAction(nameof(GetByUid), new { uid = response.AttemptUid }, response);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<AttemptResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AttemptResponse>>> GetMy(CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await attemptService.GetMyAsync(userUid, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{uid:guid}")]
    [ProducesResponseType(typeof(AttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttemptResponse>> GetByUid(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await attemptService.GetByUidAsync(uid, userUid, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{uid:guid}/answer")]
    [ProducesResponseType(typeof(SavedAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedAnswerDto>> SubmitAnswer(
        [FromRoute] Guid uid,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await attemptService.SubmitAnswerAsync(uid, userUid, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{uid:guid}/finish")]
    [ProducesResponseType(typeof(AttemptResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttemptResultResponse>> Finish(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await attemptService.FinishAsync(uid, userUid, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{uid:guid}/result")]
    [ProducesResponseType(typeof(AttemptResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttemptResultResponse>> GetResult(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var userUid = GetCurrentUserUid();
        var response = await attemptService.GetResultAsync(uid, userUid, cancellationToken);
        return Ok(response);
    }
}