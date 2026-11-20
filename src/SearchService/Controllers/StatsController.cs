using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Contracts.Statistics;
using SearchService.Services.Interfaces;
using Shared.Common.Exceptions;

namespace SearchService.Controllers;

[ApiController]
[Route("api/stats")]
[AllowAnonymous]
public class StatsController(IStatisticsService statisticsService) : ControllerBase
{
    [HttpGet("{testUid:guid}")]
    [ProducesResponseType(typeof(TestStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestStatisticsResponse>> GetByTestUid(
        [FromRoute] Guid testUid,
        CancellationToken cancellationToken)
    {
        var stats = await statisticsService.GetByTestUidAsync(testUid, cancellationToken)
            ?? throw new NotFoundException($"Статистика для теста с Uid {testUid} не найдена.");

        return Ok(stats);
    }
}