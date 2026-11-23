namespace AumoDekstop.Features.Reports.TrialBalances.Helpers;

using System;
using System.Globalization;

public static class TrialBalanceFormatter
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public static string FormatNumber(decimal n)
    {
        if (n == 0m)
        {
            return "-";
        }

        var roundedAbs = Math.Round(Math.Abs(n), 0, MidpointRounding.AwayFromZero);
        return roundedAbs.ToString("N0", IdCulture);
    }

    public static string FormatNumber(decimal? n) => FormatNumber(n ?? 0m);
}