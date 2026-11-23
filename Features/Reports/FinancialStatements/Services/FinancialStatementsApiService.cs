namespace AumoDekstop.Features.Reports.FinancialStatements.Services;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.FinancialStatements.Models;

public interface IFinancialStatementsApiService
{
    Task<IncomeStatementData?> GetIncomeStatementAsync(CancellationToken cancellationToken = default);

    Task<RetainedEarningsData?> GetRetainedEarningsAsync(CancellationToken cancellationToken = default);

    Task<CashFlowData?> GetStatementOfCashFlowAsync(CancellationToken cancellationToken = default);

    Task<FinancialPositionData?> GetStatementOfFinancialPositionAsync(
        StatementOfFinancialPositionParams? parameters = null,
        CancellationToken cancellationToken = default);
}

public sealed class FinancialStatementsApiService : IFinancialStatementsApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FinancialStatementsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<IncomeStatementData?> GetIncomeStatementAsync(CancellationToken cancellationToken = default)
        => FetchAsync<IncomeStatementData>("api/v1/reports/income-statement", cancellationToken);

    public Task<RetainedEarningsData?> GetRetainedEarningsAsync(CancellationToken cancellationToken = default)
        => FetchAsync<RetainedEarningsData>("api/v1/reports/retained-earnings", cancellationToken);

    public Task<CashFlowData?> GetStatementOfCashFlowAsync(CancellationToken cancellationToken = default)
        => FetchAsync<CashFlowData>("api/v1/reports/statement-of-cash-flow", cancellationToken);

    public Task<FinancialPositionData?> GetStatementOfFinancialPositionAsync(
        StatementOfFinancialPositionParams? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = "api/v1/reports/statement-of-financial-position";

        if (parameters?.IsPostClosing is bool isPostClosing)
        {
            var boolQuery = isPostClosing ? "true" : "false";
            requestUri = $"{requestUri}?isPostClosing={boolQuery}";
        }

        return FetchAsync<FinancialPositionData>(requestUri, cancellationToken);
    }

    private async Task<T?> FetchAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }
}