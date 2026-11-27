using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AumoDekstop.Features.Periods.ViewModels;

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
}
