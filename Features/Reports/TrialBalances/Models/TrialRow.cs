
using System.Text.Json.Serialization;
using AumoDekstop.Features.Reports.TrialBalances.Helpers;

namespace AumoDekstop.Features.Reports.TrialBalances.Models;

public sealed class RawTrialRow
{
    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("normalBalanceIsDebit")]
    public bool NormalBalanceIsDebit { get; set; }

    [JsonPropertyName("netBalance")]
    public decimal? NetBalance { get; set; }

    [JsonPropertyName("debit")]
    public decimal? Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal? Credit { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

public sealed class TrialRow
{
    public int AccountId { get; set; }

    public int ReferenceNumber { get; set; }

    public string AccountName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool NormalBalanceIsDebit { get; set; }

    public decimal? NetBalance { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public decimal? Amount { get; set; }

    // Properti bantu siap-pakai untuk x:Bind di XAML
    public string FormattedDebit => TrialBalanceFormatter.FormatNumber(Debit);

    public string FormattedCredit => TrialBalanceFormatter.FormatNumber(Credit);

    public string FormattedReferenceNumber => ReferenceNumber > 0 ? ReferenceNumber.ToString() : "-";
}