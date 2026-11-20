using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Contracts.Catalog;
using SearchService.Contracts.Common;
using SearchService.Contracts.Search;
using SearchService.Services.Interfaces;
using Shared.Common.Exceptions;

namespace SearchService.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class CatalogController(ISearchService searchService) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<SearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<SearchResponse>>> Search(
        [FromQuery] SearchTestsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await searchService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(PagedResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<CatalogItemResponse>>> GetCatalog(
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new BadRequestException("Номер страницы должен быть больше или равен 1.");
        }

        if (size is < 1 or > 100)
        {
            throw new BadRequestException("Размер страницы должен быть в диапазоне от 1 до 100.");
        }

        var result = await searchService.GetCatalogAsync(category, page, size, cancellationToken);
        return Ok(result);
    }

    [HttpGet("catalog/popular")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetPopular(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 50)
        {
            throw new BadRequestException("Параметр 'limit' должен быть в диапазоне от 1 до 50.");
        }

        var result = await searchService.GetPopularAsync(limit, cancellationToken);
        return Ok(result);
    }

    [HttpGet("catalog/new")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetNew(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 50)
        {
            throw new BadRequestException("Параметр 'limit' должен быть в диапазоне от 1 до 50.");
        }

        var result = await searchService.GetNewAsync(limit, cancellationToken);
        return Ok(result);
    }
}