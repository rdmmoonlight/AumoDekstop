
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Features.Reports.Worksheet;
public interface IWorksheetService
{
    Task<WorksheetApiResponseDto> GetWorksheetAsync(CancellationToken cancellationToken = default);
}

public class WorksheetService : IWorksheetService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;

    public WorksheetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WorksheetApiResponseDto> GetWorksheetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("/api/v1/reports/worksheet", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var serverMessage = ExtractErrorMessage(errorBody);
            throw new HttpRequestException(serverMessage ?? $"Request gagal dengan status {(int)response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<WorksheetApiResponseDto>(JsonOptions, cancellationToken);
        return result ?? new WorksheetApiResponseDto();
    }

    private static string? ExtractErrorMessage(string? jsonBody)
    {
        if (string.IsNullOrWhiteSpace(jsonBody))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonBody);
            if (doc.RootElement.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
            {
                return msgProp.GetString();
            }
            if (doc.RootElement.TryGetProperty("data", out var dataProp) &&
                dataProp.ValueKind == JsonValueKind.Object &&
                dataProp.TryGetProperty("message", out var nestedMsg) &&
                nestedMsg.ValueKind == JsonValueKind.String)
            {
                return nestedMsg.GetString();
            }
        }
        catch
        {
            // Abaikan jika body bukan JSON valid
        }

        return null;
    }
}

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