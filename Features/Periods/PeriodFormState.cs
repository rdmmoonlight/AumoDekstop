using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AumoDekstop.Features.Periods.ViewModels;

public sealed record CreatePeriodPayload(string StartDate, string EndDate);

public partial class PeriodFormState : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartDateString))]
    public partial DateTimeOffset? StartDate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EndDateString))]
    public partial DateTimeOffset? EndDate { get; set; }

    public string StartDateString => StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public string EndDateString => EndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public void ResetForm()
    {
        StartDate = null;
        EndDate = null;
    }

    public void SetFormData(CreatePeriodPayload data)
    {
        StartDate = DateTimeOffset.TryParse(
            data.StartDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsedStart)
            ? parsedStart
            : null;

        EndDate = DateTimeOffset.TryParse(
            data.EndDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsedEnd)
            ? parsedEnd
            : null;
    }

    public CreatePeriodPayload ToPayload() => new(StartDateString, EndDateString);
}
