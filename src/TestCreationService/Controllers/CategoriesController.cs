using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestCreationService.Data;

namespace TestCreationService.Controllers;

[Route("api/categories")]
[AllowAnonymous]
public class CategoriesController(TestCreationDbContext dbContext) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new { c.Uid, c.Name, c.Description })
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }
}