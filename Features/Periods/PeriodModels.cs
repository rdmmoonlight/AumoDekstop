using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AumoDekstop.Features.Periods.Helpers;

namespace AumoDekstop.Features.Periods.Models;

public class PeriodItem : INotifyPropertyChanged
{
    private bool _isClosed;
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("isClosed")]
    public bool IsClosed
    {
        get => _isClosed;
        set
        {
            if (_isClosed != value)
            {
                _isClosed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOpen));
                OnPropertyChanged(nameof(CanSelect));
            }
        }
    }

    [JsonPropertyName("isSelected")]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelect));
            }
        }
    }

    [JsonIgnore]
    public string FormattedStartDate => PeriodFormatter.FormatPeriodDate(StartDate);

    [JsonIgnore]
    public string FormattedEndDate => PeriodFormatter.FormatPeriodDate(EndDate);

    [JsonIgnore]
    public bool IsOpen => !IsClosed;

    [JsonIgnore]
    public bool CanSelect => !IsClosed && !IsSelected;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}