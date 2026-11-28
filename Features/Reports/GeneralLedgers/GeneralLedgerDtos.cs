using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AumoDekstop.Features.Reports.GeneralLedgers;

public class GetGeneralLedgerPermanentResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("isTemporary")]
    public bool IsTemporary { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("accounts")]
    public List<JsonElement>? Accounts { get; set; }
}

public class GetGeneralLedgerTemporaryResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("isTemporary")]
    public bool IsTemporary { get; set; }

    [JsonPropertyName("netIncomeBeforeClosing")]
    public decimal? NetIncomeBeforeClosing { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("accounts")]
    public List<JsonElement>? Accounts { get; set; }
}

public class RefreshGeneralLedgerResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Menampung properti tambahan dinamis ([key: string]: unknown)
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}