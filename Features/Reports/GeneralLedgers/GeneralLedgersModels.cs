namespace AumoDekstop.Features.Reports.GeneralLedgers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

#region Formatter Helpers (Konversi dari format_2.ts)

public static class GeneralLedgerFormatter
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    /// <summary>
    /// Memformat angka ke format mata uang Rupiah (IDR)
    /// </summary>
    public static string FormatCurrency(decimal? amount)
    {
        if (!amount.HasValue)
        {
            return "Rp 0";
        }

        // Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0, maximumFractionDigits: 2 })
        var formattedNumber = amount.Value.ToString("#,##0.##", IdCulture);
        return amount.Value < 0
            ? $"-Rp {Math.Abs(amount.Value).ToString("#,##0.##", IdCulture)}"
            : $"Rp {formattedNumber}";
    }

    /// <summary>
    /// Memformat angka mutasi debit/kredit (menampilkan '-' jika bernilai 0 atau null)
    /// </summary>
    public static string FormatMutation(decimal? amount)
    {
        if (!amount.HasValue || amount.Value == 0m)
        {
            return "-";
        }

        return amount.Value.ToString("#,##0.##", IdCulture);
    }

    /// <summary>
    /// Memformat tanggal ISO string ke format lokal Indonesia (Contoh: 12 Nov 2026)
    /// </summary>
    public static string FormatDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return "-";
        }

        if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
        {
            return dateString;
        }

        return date.ToString("dd MMM yyyy", IdCulture);
    }

    public static string FormatDate(DateTime? date)
    {
        return date.HasValue ? date.Value.ToString("dd MMM yyyy", IdCulture) : "-";
    }
}

#endregion

#region Enums & Models (Konversi dari types_2.ts)

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountNormalBalance
{
    DEBIT,
    CREDIT
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountCategory
{
    ASSET,
    LIABILITY,
    EQUITY,
    REVENUE,
    EXPENSE
}

public class LedgerEntryItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("referenceNumber")]
    public string ReferenceNumber { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    // --- Helper Properti Siap Pakai untuk x:Bind di XAML ---

    [JsonIgnore]
    public string FormattedDate => GeneralLedgerFormatter.FormatDate(Date);

    [JsonIgnore]
    public string FormattedDebit => GeneralLedgerFormatter.FormatMutation(Debit);

    [JsonIgnore]
    public string FormattedCredit => GeneralLedgerFormatter.FormatMutation(Credit);

    [JsonIgnore]
    public string FormattedBalance => GeneralLedgerFormatter.FormatCurrency(Balance);

    [JsonIgnore]
    public string ReferenceDisplay => string.IsNullOrWhiteSpace(ReferenceNumber) ? "-" : ReferenceNumber;
}

public class LedgerAccountGroup
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("accountCode")]
    public string AccountCode { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public AccountCategory Category { get; set; }

    [JsonPropertyName("normalBalance")]
    public AccountNormalBalance NormalBalance { get; set; }

    [JsonPropertyName("beginningBalance")]
    public decimal BeginningBalance { get; set; }

    [JsonPropertyName("endingBalance")]
    public decimal EndingBalance { get; set; }

    [JsonPropertyName("totalDebit")]
    public decimal TotalDebit { get; set; }

    [JsonPropertyName("totalCredit")]
    public decimal TotalCredit { get; set; }

    [JsonPropertyName("entries")]
    public List<LedgerEntryItem> Entries { get; set; } = [];

    // --- Helper Properti Siap Pakai untuk x:Bind di XAML ---

    [JsonIgnore]
    public string FormattedBeginningBalance => GeneralLedgerFormatter.FormatCurrency(BeginningBalance);

    [JsonIgnore]
    public string FormattedEndingBalance => GeneralLedgerFormatter.FormatCurrency(EndingBalance);

    [JsonIgnore]
    public string FormattedTotalDebit => GeneralLedgerFormatter.FormatMutation(TotalDebit);

    [JsonIgnore]
    public string FormattedTotalCredit => GeneralLedgerFormatter.FormatMutation(TotalCredit);
}

public class GeneralLedgerSummary
{
    [JsonPropertyName("totalRevenue")]
    public decimal? TotalRevenue { get; set; }

    [JsonPropertyName("totalExpense")]
    public decimal? TotalExpense { get; set; }

    [JsonPropertyName("netIncome")]
    public decimal? NetIncome { get; set; }

    // --- Helper Properti Siap Pakai untuk x:Bind di XAML ---

    [JsonIgnore]
    public string FormattedTotalRevenue => GeneralLedgerFormatter.FormatCurrency(TotalRevenue);

    [JsonIgnore]
    public string FormattedTotalExpense => GeneralLedgerFormatter.FormatCurrency(TotalExpense);

    [JsonIgnore]
    public string FormattedNetIncome => GeneralLedgerFormatter.FormatCurrency(NetIncome);
}

public class GeneralLedgerResponse
{
    [JsonPropertyName("periodId")]
    public string PeriodId { get; set; } = string.Empty;

    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("isClosed")]
    public bool IsClosed { get; set; }

    [JsonPropertyName("accounts")]
    public List<LedgerAccountGroup> Accounts { get; set; } = [];

    [JsonPropertyName("summary")]
    public GeneralLedgerSummary? Summary { get; set; }

    // --- Helper Properti Siap Pakai untuk x:Bind di XAML ---

    [JsonIgnore]
    public string FormattedStartDate => GeneralLedgerFormatter.FormatDate(StartDate);

    [JsonIgnore]
    public string FormattedEndDate => GeneralLedgerFormatter.FormatDate(EndDate);
}

#endregion