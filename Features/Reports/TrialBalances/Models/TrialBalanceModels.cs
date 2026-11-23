namespace AumoDekstop.Features.Reports.TrialBalances.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public enum TrialBalanceType
{
    Unadjusted,
    Adjusted,
    PostClosing
}

public static class TrialBalanceTypeExtensions
{
    public static string ToApiQueryValue(this TrialBalanceType type) => type switch
    {
        TrialBalanceType.Unadjusted => "unadjusted",
        TrialBalanceType.Adjusted => "adjusted",
        TrialBalanceType.PostClosing => "post-closing",
        _ => "unadjusted"
    };
}

public sealed class GetTrialBalanceParams
{
    public TrialBalanceType? Type { get; set; }
}

// Sesuaikan properti di bawah ini dengan skema JSON dari endpoint /api/v1/reports/trial-balances
public sealed class TrialBalanceResponseDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<TrialBalanceItemDto> Items { get; set; } = [];

    [JsonPropertyName("totalDebit")]
    public decimal TotalDebit { get; set; }

    [JsonPropertyName("totalCredit")]
    public decimal TotalCredit { get; set; }
}

public sealed class TrialBalanceItemDto
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("accountCode")]
    public string AccountCode { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }
}