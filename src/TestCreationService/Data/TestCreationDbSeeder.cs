using Microsoft.EntityFrameworkCore;
using TestCreationService.Domain.Entities;

namespace TestCreationService.Data;

public static class TestCreationDbSeeder
{
    public static async Task SeedCategoriesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestCreationDbContext>();

        if (await dbContext.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new()
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Name = "Программирование",
                Description = "Алгоритмы, структуры данных, языки программирования и разработка ПО"
            },
            new()
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Name = "Базы данных",
                Description = "Реляционные и NoSQL СУБД, SQL, проектирование схем и оптимизация запросов"
            },
            new()
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Name = "Компьютерные сети",
                Description = "Сетевые протоколы, архитектура TCP/IP, маршрутизация и безопасность"
            },
            new()
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                Name = "Математика",
                Description = "Дискретная математика, линейная алгебра, теория вероятностей и матстатистика"
            }
        };

        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();
    }
}