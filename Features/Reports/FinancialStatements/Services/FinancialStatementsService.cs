using System;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Reports.FinancialStatements.Models;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Reports.FinancialStatements.Services;

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
    private readonly ApiService _apiService;

    public FinancialStatementsApiService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
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
        var response = await _apiService.GetAsync<T>(requestUri, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request failed with status {response.StatusCode}.");
        }

        return response.Data;
    }
}