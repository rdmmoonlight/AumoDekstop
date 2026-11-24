
using System;
using System.Globalization;

namespace AumoDekstop.Features.Reports.FinancialStatements.Helpers;
public static class FinancialStatementFormatter
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    /// <summary>
    /// Memformat angka menjadi format mata uang IDR (Rupiah).
    /// </summary>
    public static string FormatCurrency(decimal? value)
    {
        if (value is null)
        {
            return "Rp 0";
        }

        var rounded = Math.Round(value.Value, 0, MidpointRounding.AwayFromZero);
        return rounded.ToString("C0", IdCulture);
    }

    /// <summary>
    /// Memformat persentase (nilai input 0-100 dibagi 100, maksimal 2 desimal).
    /// </summary>
    public static string FormatPercent(decimal? value)
    {
        if (value is null)
        {
            return "0%";
        }

        var ratio = value.Value / 100m;
        return ratio.ToString("0.##%", IdCulture);
    }
}