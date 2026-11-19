using System.Net;
using System.Net.Http.Json;
using Shared.Common.Exceptions;
using TestTakingService.Contracts.Snapshots;
using TestTakingService.Services.Interfaces;

namespace TestTakingService.Infrastructure.HttpClients;

public class TestCreationHttpClient(HttpClient httpClient) : ITestCreationClient
{
    public async Task<TestGradingSnapshot> GetTestSnapshotAsync(
        Guid testUid,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/tests/{testUid}/snapshot", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException($"Тест с Uid {testUid} не найден.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException("Тест скрыт или недоступен для прохождения.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException(
                $"Не удалось получить данные теста из сервиса создания тестов (HTTP {(int)response.StatusCode}).");
        }

        var snapshot = await response.Content.ReadFromJsonAsync<TestGradingSnapshot>(cancellationToken)
            ?? throw new InvalidOperationException($"Получен пустой снапшот для теста {testUid}.");

        return snapshot;
    }
}