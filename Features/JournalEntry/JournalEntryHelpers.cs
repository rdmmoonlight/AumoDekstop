namespace AumoDekstop.Features.JournalEntries.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;

public interface IJournalAmountLine
{
    decimal Debit { get; }
    decimal Credit { get; }
}

public static class JournalEntriesHelpers
{
    private static readonly CultureInfo IndonesianCulture = new("id-ID");

    public static string FormatCurrency(decimal amount)
    {
        return string.Format(IndonesianCulture, "{0:C0}", amount);
    }

    public static string FormatDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return string.Empty;
        }

        if (DateTimeOffset.TryParse(dateString, out var date))
        {
            return date.ToString("d MMMM yyyy", IndonesianCulture);
        }

        return string.Empty;
    }

    public static (decimal TotalDebit, decimal TotalCredit) CalculateTotals<T>(IEnumerable<T>? lines)
        where T : IJournalAmountLine
    {
        if (lines is null)
        {
            return (0m, 0m);
        }

        decimal totalDebit = 0m;
        decimal totalCredit = 0m;

        foreach (var line in lines)
        {
            totalDebit += line.Debit;
            totalCredit += line.Credit;
        }

        return (totalDebit, totalCredit);
    }
}