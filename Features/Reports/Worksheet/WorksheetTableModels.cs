namespace AumoDekstop.Features.Reports.Worksheet;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

public static class WorksheetFormatter
{
    private static readonly CultureInfo IndonesianCulture = new("id-ID");

    public static string FormatNumber(decimal n)
    {
        if (n == 0)
        {
            return "-";
        }

        return Math.Abs(n).ToString("N0", IndonesianCulture);
    }
}

public class WorksheetRow
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

    [JsonPropertyName("unadjustedDebit")]
    public decimal UnadjustedDebit { get; set; }

    [JsonPropertyName("unadjustedCredit")]
    public decimal UnadjustedCredit { get; set; }

    [JsonPropertyName("adjustmentDebit")]
    public decimal AdjustmentDebit { get; set; }

    [JsonPropertyName("adjustmentCredit")]
    public decimal AdjustmentCredit { get; set; }

    [JsonPropertyName("adjustedDebit")]
    public decimal AdjustedDebit { get; set; }

    [JsonPropertyName("adjustedCredit")]
    public decimal AdjustedCredit { get; set; }

    [JsonPropertyName("incomeStatementDebit")]
    public decimal IncomeStatementDebit { get; set; }

    [JsonPropertyName("incomeStatementCredit")]
    public decimal IncomeStatementCredit { get; set; }

    [JsonPropertyName("financialPositionDebit")]
    public decimal FinancialPositionDebit { get; set; }

    [JsonPropertyName("financialPositionCredit")]
    public decimal FinancialPositionCredit { get; set; }

    [JsonIgnore]
    public string UnadjustedDebitDisplay => WorksheetFormatter.FormatNumber(UnadjustedDebit);

    [JsonIgnore]
    public string UnadjustedCreditDisplay => WorksheetFormatter.FormatNumber(UnadjustedCredit);

    [JsonIgnore]
    public string AdjustmentDebitDisplay => WorksheetFormatter.FormatNumber(AdjustmentDebit);

    [JsonIgnore]
    public string AdjustmentCreditDisplay => WorksheetFormatter.FormatNumber(AdjustmentCredit);

    [JsonIgnore]
    public string AdjustedDebitDisplay => WorksheetFormatter.FormatNumber(AdjustedDebit);

    [JsonIgnore]
    public string AdjustedCreditDisplay => WorksheetFormatter.FormatNumber(AdjustedCredit);

    [JsonIgnore]
    public string IncomeStatementDebitDisplay => WorksheetFormatter.FormatNumber(IncomeStatementDebit);

    [JsonIgnore]
    public string IncomeStatementCreditDisplay => WorksheetFormatter.FormatNumber(IncomeStatementCredit);

    [JsonIgnore]
    public string FinancialPositionDebitDisplay => WorksheetFormatter.FormatNumber(FinancialPositionDebit);

    [JsonIgnore]
    public string FinancialPositionCreditDisplay => WorksheetFormatter.FormatNumber(FinancialPositionCredit);
}

public class WorksheetTotals
{
    public decimal UnadjustedDebit { get; set; }
    public decimal UnadjustedCredit { get; set; }
    public decimal AdjustmentDebit { get; set; }
    public decimal AdjustmentCredit { get; set; }
    public decimal AdjustedDebit { get; set; }
    public decimal AdjustedCredit { get; set; }
    public decimal IsDebit { get; set; }
    public decimal IsCredit { get; set; }
    public decimal BsDebit { get; set; }
    public decimal BsCredit { get; set; }
    public decimal NetIncome { get; set; }

    // Baris 1 Footer: Total
    public string UnadjustedDebitDisplay => WorksheetFormatter.FormatNumber(UnadjustedDebit);
    public string UnadjustedCreditDisplay => WorksheetFormatter.FormatNumber(UnadjustedCredit);
    public string AdjustmentDebitDisplay => WorksheetFormatter.FormatNumber(AdjustmentDebit);
    public string AdjustmentCreditDisplay => WorksheetFormatter.FormatNumber(AdjustmentCredit);
    public string AdjustedDebitDisplay => WorksheetFormatter.FormatNumber(AdjustedDebit);
    public string AdjustedCreditDisplay => WorksheetFormatter.FormatNumber(AdjustedCredit);
    public string IsDebitDisplay => WorksheetFormatter.FormatNumber(IsDebit);
    public string IsCreditDisplay => WorksheetFormatter.FormatNumber(IsCredit);
    public string BsDebitDisplay => WorksheetFormatter.FormatNumber(BsDebit);
    public string BsCreditDisplay => WorksheetFormatter.FormatNumber(BsCredit);

    // Baris 2 Footer: Net Income (plug)
    public string NetIncomeIsDebitDisplay => NetIncome >= 0 ? WorksheetFormatter.FormatNumber(NetIncome) : "-";
    public string NetIncomeIsCreditDisplay => NetIncome < 0 ? WorksheetFormatter.FormatNumber(Math.Abs(NetIncome)) : "-";
    public string NetIncomeBsDebitDisplay => NetIncome < 0 ? WorksheetFormatter.FormatNumber(Math.Abs(NetIncome)) : "-";
    public string NetIncomeBsCreditDisplay => NetIncome >= 0 ? WorksheetFormatter.FormatNumber(NetIncome) : "-";

    // Baris 3 Footer: Total (after plug)
    public string AfterPlugIsDebitDisplay => WorksheetFormatter.FormatNumber(IsDebit + (NetIncome >= 0 ? NetIncome : 0));
    public string AfterPlugIsCreditDisplay => WorksheetFormatter.FormatNumber(IsCredit + (NetIncome < 0 ? Math.Abs(NetIncome) : 0));
    public string AfterPlugBsDebitDisplay => WorksheetFormatter.FormatNumber(BsDebit + (NetIncome < 0 ? Math.Abs(NetIncome) : 0));
    public string AfterPlugBsCreditDisplay => WorksheetFormatter.FormatNumber(BsCredit + (NetIncome >= 0 ? NetIncome : 0));
}

public class WorksheetData
{
    [JsonPropertyName("rows")]
    public List<WorksheetRow> Rows { get; set; } = new();

    [JsonPropertyName("netIncome")]
    public decimal NetIncome { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }
}