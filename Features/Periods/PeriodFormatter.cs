using System.Globalization;

namespace AumoDekstop.Features.Periods.Helpers;

public static class PeriodFormatter
{
    private static readonly CultureInfo IndonesianCulture = new("id-ID");

    /// <summary>
    /// Memformat tanggal ISO dari API (UTC) menjadi format Indonesia.
    /// Contoh: "2026-11-01T00:00:00Z" -> "1 November 2026".
    /// </summary>
    public static string FormatPeriodDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var date))
        {
            return value;
        }

        // Format "d MMMM yyyy" pada culture id-ID menghasilkan "1 November 2026"
        return date.UtcDateTime.ToString("d MMMM yyyy", IndonesianCulture);
    }
}
