namespace AumoDekstop.Features.Reports.FinancialStatements.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using AumoDekstop.Features.Reports.FinancialStatements.Helpers;

// --- Common / Shared Types ---

public sealed class FinancialPeriod
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Nilai umum: "OPEN" atau "CLOSED"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    public bool IsClosed => string.Equals(Status, "CLOSED", System.StringComparison.OrdinalIgnoreCase);

    public string PeriodRangeLabel => $"{StartDate} - {EndDate}";
}

// --- Income Statement (Laporan Laba Rugi) ---

public sealed class IncomeStatementItem
{
    [JsonPropertyName("accountCode")]
    public string AccountCode { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    public string FormattedAmount => FinancialStatementFormatter.FormatCurrency(Amount);
}

public sealed class IncomeStatementSection
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<IncomeStatementItem> Items { get; set; } = [];

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    public string FormattedSubtotal => FinancialStatementFormatter.FormatCurrency(Subtotal);

    public string UpperTitle => Title?.ToUpperInvariant() ?? string.Empty;

    public string TotalSectionLabel => $"Total {Title}";

    public bool HasItems => Items is { Count: > 0 };
}

public sealed class IncomeStatementData
{
    [JsonPropertyName("period")]
    public FinancialPeriod Period { get; set; } = new();

    [JsonPropertyName("revenue")]
    public IncomeStatementSection Revenue { get; set; } = new();

    [JsonPropertyName("costOfGoodsSold")]
    public IncomeStatementSection CostOfGoodsSold { get; set; } = new();

    [JsonPropertyName("grossProfit")]
    public decimal GrossProfit { get; set; }

    [JsonPropertyName("operatingExpenses")]
    public IncomeStatementSection OperatingExpenses { get; set; } = new();

    [JsonPropertyName("operatingIncome")]
    public decimal OperatingIncome { get; set; }

    [JsonPropertyName("otherIncomeExpense")]
    public IncomeStatementSection OtherIncomeExpense { get; set; } = new();

    [JsonPropertyName("netIncomeBeforeTax")]
    public decimal NetIncomeBeforeTax { get; set; }

    [JsonPropertyName("taxExpense")]
    public decimal TaxExpense { get; set; }

    [JsonPropertyName("netIncome")]
    public decimal NetIncome { get; set; }

    public string FormattedGrossProfit => FinancialStatementFormatter.FormatCurrency(GrossProfit);
    public string FormattedOperatingIncome => FinancialStatementFormatter.FormatCurrency(OperatingIncome);
    public string FormattedNetIncomeBeforeTax => FinancialStatementFormatter.FormatCurrency(NetIncomeBeforeTax);
    public string FormattedTaxExpense => FinancialStatementFormatter.FormatCurrency(TaxExpense);
    public string FormattedNetIncome => FinancialStatementFormatter.FormatCurrency(NetIncome);
}

// --- Retained Earnings (Laporan Laba Ditahan) ---

public sealed class RetainedEarningsData
{
    [JsonPropertyName("period")]
    public FinancialPeriod Period { get; set; } = new();

    [JsonPropertyName("beginningBalance")]
    public decimal BeginningBalance { get; set; }

    [JsonPropertyName("netIncome")]
    public decimal NetIncome { get; set; }

    [JsonPropertyName("dividendsPaid")]
    public decimal DividendsPaid { get; set; }

    [JsonPropertyName("otherAdjustments")]
    public decimal OtherAdjustments { get; set; }

    [JsonPropertyName("endingBalance")]
    public decimal EndingBalance { get; set; }

    public string FormattedBeginningBalance => FinancialStatementFormatter.FormatCurrency(BeginningBalance);
    public string FormattedNetIncome => FinancialStatementFormatter.FormatCurrency(NetIncome);
    public string FormattedDividendsPaid => FinancialStatementFormatter.FormatCurrency(DividendsPaid);
    public string FormattedOtherAdjustments => FinancialStatementFormatter.FormatCurrency(OtherAdjustments);
    public string FormattedEndingBalance => FinancialStatementFormatter.FormatCurrency(EndingBalance);
}

// --- Statement of Cash Flow (Laporan Arus Kas) ---

public sealed class CashFlowItem
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    public string FormattedAmount => FinancialStatementFormatter.FormatCurrency(Amount);
}

public sealed class CashFlowSection
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<CashFlowItem> Items { get; set; } = [];

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    public string FormattedSubtotal => FinancialStatementFormatter.FormatCurrency(Subtotal);

    public string UpperTitle => Title?.ToUpperInvariant() ?? string.Empty;

    public string NetCashFlowLabel => $"Arus Kas Bersih dari {Title}";
}

public sealed class CashFlowData
{
    [JsonPropertyName("period")]
    public FinancialPeriod Period { get; set; } = new();

    [JsonPropertyName("operatingActivities")]
    public CashFlowSection OperatingActivities { get; set; } = new();

    [JsonPropertyName("investingActivities")]
    public CashFlowSection InvestingActivities { get; set; } = new();

    [JsonPropertyName("financingActivities")]
    public CashFlowSection FinancingActivities { get; set; } = new();

    [JsonPropertyName("netChangeInCash")]
    public decimal NetChangeInCash { get; set; }

    [JsonPropertyName("beginningCashBalance")]
    public decimal BeginningCashBalance { get; set; }

    [JsonPropertyName("endingCashBalance")]
    public decimal EndingCashBalance { get; set; }

    public string FormattedNetChangeInCash => FinancialStatementFormatter.FormatCurrency(NetChangeInCash);
    public string FormattedBeginningCashBalance => FinancialStatementFormatter.FormatCurrency(BeginningCashBalance);
    public string FormattedEndingCashBalance => FinancialStatementFormatter.FormatCurrency(EndingCashBalance);
}

// --- Statement of Financial Position / Balance Sheet (Laporan Posisi Keuangan) ---

public sealed class BalanceSheetItem
{
    [JsonPropertyName("accountCode")]
    public string AccountCode { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    public string FormattedAmount => FinancialStatementFormatter.FormatCurrency(Amount);
}

public sealed class BalanceSheetSection
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<BalanceSheetItem> Items { get; set; } = [];

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    public string FormattedSubtotal => FinancialStatementFormatter.FormatCurrency(Subtotal);

    public string UpperTitle => Title?.ToUpperInvariant() ?? string.Empty;

    public string TotalSectionLabel => $"Total {Title}";
}

public sealed class BalanceSheetCategory
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("sections")]
    public List<BalanceSheetSection> Sections { get; set; } = [];

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    public string FormattedTotal => FinancialStatementFormatter.FormatCurrency(Total);
}

public sealed class BalanceSheetAssetsGroup
{
    [JsonPropertyName("currentAssets")]
    public BalanceSheetSection CurrentAssets { get; set; } = new();

    [JsonPropertyName("nonCurrentAssets")]
    public BalanceSheetSection NonCurrentAssets { get; set; } = new();

    [JsonPropertyName("totalAssets")]
    public decimal TotalAssets { get; set; }

    public string FormattedTotalAssets => FinancialStatementFormatter.FormatCurrency(TotalAssets);
}

public sealed class BalanceSheetLiabilitiesGroup
{
    [JsonPropertyName("currentLiabilities")]
    public BalanceSheetSection CurrentLiabilities { get; set; } = new();

    [JsonPropertyName("nonCurrentLiabilities")]
    public BalanceSheetSection NonCurrentLiabilities { get; set; } = new();

    [JsonPropertyName("totalLiabilities")]
    public decimal TotalLiabilities { get; set; }

    public string FormattedTotalLiabilities => FinancialStatementFormatter.FormatCurrency(TotalLiabilities);
}

public sealed class BalanceSheetEquityGroup
{
    [JsonPropertyName("section")]
    public BalanceSheetSection Section { get; set; } = new();

    [JsonPropertyName("totalEquity")]
    public decimal TotalEquity { get; set; }

    public string FormattedTotalEquity => FinancialStatementFormatter.FormatCurrency(TotalEquity);
}

public sealed class FinancialPositionData
{
    [JsonPropertyName("period")]
    public FinancialPeriod Period { get; set; } = new();

    [JsonPropertyName("assets")]
    public BalanceSheetAssetsGroup Assets { get; set; } = new();

    [JsonPropertyName("liabilities")]
    public BalanceSheetLiabilitiesGroup Liabilities { get; set; } = new();

    [JsonPropertyName("equity")]
    public BalanceSheetEquityGroup Equity { get; set; } = new();

    [JsonPropertyName("totalLiabilitiesAndEquity")]
    public decimal TotalLiabilitiesAndEquity { get; set; }

    public string FormattedTotalLiabilitiesAndEquity => FinancialStatementFormatter.FormatCurrency(TotalLiabilitiesAndEquity);
}