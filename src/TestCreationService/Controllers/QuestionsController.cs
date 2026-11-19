using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestCreationService.Contracts.Questions;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Controllers;

[Route("api/questions")]
[Authorize]
public class QuestionsController(IQuestionService questionService) : BaseApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponse>> Create(
        [FromBody] CreateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await questionService.CreateAsync(authorUid, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Uid }, response);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<QuestionResponse>>> GetMy(CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await questionService.GetMyAsync(authorUid, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var requesterUid = GetCurrentUserUid();
        var response = await questionService.GetByIdAsync(id, requesterUid, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await questionService.UpdateAsync(id, authorUid, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        await questionService.DeleteAsync(id, authorUid, cancellationToken);
        return NoContent();
    }
}