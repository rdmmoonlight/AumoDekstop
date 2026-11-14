using System.Text.Json.Serialization;
using AumoDekstop.Features.Periods.Helpers;

namespace AumoDekstop.Features.Periods.Models;

public sealed class PeriodItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("isClosed")]
    public bool IsClosed { get; set; }

    [JsonPropertyName("isSelected")]
    public bool IsSelected { get; set; }

    // Properti bantuan (computed) untuk langsung di-bind di XAML
    [JsonIgnore]
    public string FormattedStartDate => PeriodFormatter.FormatPeriodDate(StartDate);

    [JsonIgnore]
    public string FormattedEndDate => PeriodFormatter.FormatPeriodDate(EndDate);
}

public sealed class GetPeriodsResponse
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("selectedPeriodId")]
    public int? SelectedPeriodId { get; set; }

    [JsonPropertyName("periods")]
    public List<PeriodItem> Periods { get; set; } = [];
}

public sealed class AccountSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayLabel => $"{Code} - {Name}";
}

public sealed class PeriodsOpenInfo
{
    [JsonPropertyName("hasExistingPermanentAccounts")]
    public bool HasExistingPermanentAccounts { get; set; }

    [JsonPropertyName("availableCashAndBankAccounts")]
    public List<AccountSummary>? AvailableCashAndBankAccounts { get; set; }

    [JsonPropertyName("availableRetainedEarningsAccounts")]
    public List<AccountSummary>? AvailableRetainedEarningsAccounts { get; set; }
}
