using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestCreationService.Contracts.Tests;
using TestCreationService.Services.Interfaces;

namespace TestCreationService.Controllers;

[Route("api/tests")]
[Authorize]
public class TestsController(
    ITestService testService,
    IConfiguration configuration) : BaseApiController
{
    public const string InternalApiKeyHeader = "X-Internal-Api-Key";

    [HttpPost]
    [ProducesResponseType(typeof(TestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestResponse>> Create(
        [FromBody] CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await testService.CreateAsync(authorUid, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{uid:guid}")]
    [ProducesResponseType(typeof(TestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestResponse>> Update(
        [FromRoute] Guid uid,
        [FromBody] UpdateTestRequest request,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await testService.UpdateAsync(uid, authorUid, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{uid:guid}/publish")]
    [ProducesResponseType(typeof(PublishTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PublishTestResponse>> Publish(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await testService.PublishAsync(uid, authorUid, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{uid:guid}/unpublish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unpublish(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        await testService.UnpublishAsync(uid, authorUid, cancellationToken);
        return NoContent();
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<TestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TestResponse>>> GetMy(CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await testService.GetMyAsync(authorUid, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{uid:guid}")]
    [ProducesResponseType(typeof(TestDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDetailResponse>> GetById(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var authorUid = GetCurrentUserUid();
        var response = await testService.GetByIdAsync(uid, authorUid, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{uid:guid}/snapshot")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TestSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestSnapshotResponse>> GetSnapshot(
        [FromRoute] Guid uid,
        CancellationToken cancellationToken)
    {
        var includeCorrectAnswers = HasValidInternalApiKey();
        var response = await testService.GetSnapshotAsync(uid, includeCorrectAnswers, cancellationToken);
        return Ok(response);
    }

    private bool HasValidInternalApiKey()
    {
        var configuredKey = configuration["InternalAuth:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return false;

        if (!Request.Headers.TryGetValue(InternalApiKeyHeader, out var providedHeader))
            return false;

        var providedKey = providedHeader.ToString();
        if (string.IsNullOrWhiteSpace(providedKey))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configuredKey),
            Encoding.UTF8.GetBytes(providedKey));
    }
}