
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AumoDekstop.Features.Periods.Helpers;

namespace AumoDekstop.Features.Periods.Models;

public class PeriodItem : INotifyPropertyChanged
{
    private int _id;
    private string _periodName = string.Empty;
    private string _startDate = string.Empty;
    private string _endDate = string.Empty;
    private bool _isClosed;
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("id")]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [JsonPropertyName("periodName")]
    public string PeriodName
    {
        get => _periodName;
        set => SetProperty(ref _periodName, value);
    }

    [JsonPropertyName("startDate")]
    public string StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                OnPropertyChanged(nameof(FormattedStartDate));
            }
        }
    }

    [JsonPropertyName("endDate")]
    public string EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                OnPropertyChanged(nameof(FormattedEndDate));
            }
        }
    }

    [JsonPropertyName("isClosed")]
    public bool IsClosed
    {
        get => _isClosed;
        set
        {
            if (SetProperty(ref _isClosed, value))
            {
                OnPropertyChanged(nameof(IsOpen));
                OnPropertyChanged(nameof(HasAnyAction));
            }
        }
    }

    [JsonPropertyName("isSelected")]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(CanSelect));
                OnPropertyChanged(nameof(HasAnyAction));
            }
        }
    }

    // Properti bantuan (computed) untuk langsung di-bind di XAML
    [JsonIgnore]
    public string FormattedStartDate => PeriodFormatter.FormatPeriodDate(StartDate);

    [JsonIgnore]
    public string FormattedEndDate => PeriodFormatter.FormatPeriodDate(EndDate);

    // Dipakai oleh PeriodsTableControl.xaml (IsOpen, CanSelect)
    [JsonIgnore]
    public bool IsOpen => !IsClosed;

    [JsonIgnore]
    public bool CanSelect => !IsSelected;

    [JsonIgnore]
    public bool HasAnyAction => IsOpen || CanSelect;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class GetPeriodsResponse
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("selectedPeriodId")]
    public int? SelectedPeriodId { get; set; }

    [JsonPropertyName("periods")]
    public List<PeriodItem> Periods { get; set; } = new List<PeriodItem>();
}

public class AccountSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayLabel => $"{Code} - {Name}";
}

public class PeriodsOpenInfo
{
    [JsonPropertyName("hasExistingPermanentAccounts")]
    public bool HasExistingPermanentAccounts { get; set; }

    [JsonPropertyName("availableCashAndBankAccounts")]
    public List<AccountSummary>? AvailableCashAndBankAccounts { get; set; }

    [JsonPropertyName("availableRetainedEarningsAccounts")]
    public List<AccountSummary>? AvailableRetainedEarningsAccounts { get; set; }
}
