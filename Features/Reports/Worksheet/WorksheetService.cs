using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Services.Api;

namespace AumoDekstop.Features.Reports.Worksheet;

public interface IWorksheetService
{
    Task<WorksheetApiResponseDto> GetWorksheetAsync(CancellationToken cancellationToken = default);
}

public class WorksheetService : IWorksheetService
{
    private readonly ApiService _apiService;

    public WorksheetService(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    public async Task<WorksheetApiResponseDto> GetWorksheetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiService.GetAsync<WorksheetApiResponseDto>("api/v1/reports/worksheet", cancellationToken);

        if (!response.IsSuccess)
        {
            throw new InvalidOperationException(response.ErrorMessage ?? $"Request gagal dengan status {response.StatusCode}.");
        }

        return response.Data ?? new WorksheetApiResponseDto();
    }
}

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class WorksheetApiRowDto
{
    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("normalBalanceIsDebit")]
    public bool? NormalBalanceIsDebit { get; set; }

    [JsonPropertyName("tbDebit")]
    public decimal? TbDebit { get; set; }

    [JsonPropertyName("tbCredit")]
    public decimal? TbCredit { get; set; }

    [JsonPropertyName("adjDebit")]
    public decimal? AdjDebit { get; set; }

    [JsonPropertyName("adjCredit")]
    public decimal? AdjCredit { get; set; }

    [JsonPropertyName("adjTbDebit")]
    public decimal? AdjTbDebit { get; set; }

    [JsonPropertyName("adjTbCredit")]
    public decimal? AdjTbCredit { get; set; }

    [JsonPropertyName("isDebit")]
    public decimal? IsDebit { get; set; }

    [JsonPropertyName("isCredit")]
    public decimal? IsCredit { get; set; }

    [JsonPropertyName("bsDebit")]
    public decimal? BsDebit { get; set; }

    [JsonPropertyName("bsCredit")]
    public decimal? BsCredit { get; set; }
}

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class WorksheetApiTotalsDto
{
    [JsonPropertyName("netIncome")]
    public decimal? NetIncome { get; set; }
}

public class WorksheetApiResponseDto
{
    [JsonPropertyName("rows")]
    public List<WorksheetApiRowDto>? Rows { get; set; }

    [JsonPropertyName("totals")]
    public WorksheetApiTotalsDto? Totals { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool? HasPeriodSelected { get; set; }
}