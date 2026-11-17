using System.Collections.Specialized;
using System.Globalization;
using AumoDekstop.Features.ChartOfAccounts.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AumoDekstop.Features.ChartOfAccounts.Controls;

public sealed partial class AccountsTableControl : UserControl
{
    public event EventHandler<Account>? EditAccountRequested;
    public event EventHandler<Account>? DeleteAccountRequested;

    public static readonly DependencyProperty AccountsProperty =
        DependencyProperty.Register(
            nameof(Accounts),
            typeof(IEnumerable<Account>),
            typeof(AccountsTableControl),
            new PropertyMetadata(Enumerable.Empty<Account>(), OnStateChanged));

    public IEnumerable<Account> Accounts
    {
        get => (IEnumerable<Account>)GetValue(AccountsProperty);
        set => SetValue(AccountsProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AccountsTableControl),
            new PropertyMetadata(false, OnStateChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public AccountsTableControl()
    {
        // Daftarkan resource converters sebelum InitializeComponent
        Resources["BoolToOpacityConverter"] = new BoolToOpacityConverter();
        Resources["AccountTypeToBackgroundConverter"] = new AccountTypeToBackgroundConverter();
        Resources["AccountTypeToForegroundConverter"] = new AccountTypeToForegroundConverter();
        Resources["AccountRoleDisplayConverter"] = new AccountRoleDisplayConverter();
        Resources["StatusToBackgroundConverter"] = new StatusToBackgroundConverter();
        Resources["StatusToTextConverter"] = new StatusToTextConverter();
        Resources["CurrencyIdrConverter"] = new CurrencyIdrConverter();

        this.InitializeComponent();
        UpdateVisualStates();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AccountsTableControl control)
        {
            if (e.Property == AccountsProperty)
            {
                if (e.OldValue is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= control.OnAccountsCollectionChanged;
                }
                if (e.NewValue is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += control.OnAccountsCollectionChanged;
                }
            }
            control.UpdateVisualStates();
        }
    }

    private void OnAccountsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVisualStates();
    }

    private void UpdateVisualStates()
    {
        if (LoadingPanel == null || EmptyStateText == null || AccountsListView == null)
            return;

        if (IsLoading)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EmptyStateText.Visibility = Visibility.Collapsed;
            AccountsListView.Visibility = Visibility.Collapsed;
            return;
        }

        LoadingPanel.Visibility = Visibility.Collapsed;
        bool hasItems = Accounts != null && Accounts.Any();

        EmptyStateText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        AccountsListView.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is Account account)
        {
            EditAccountRequested?.Invoke(this, account);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is Account account)
        {
            DeleteAccountRequested?.Invoke(this, account);
        }
    }
}

#region Value Converters untuk Tampilan Tabel

public class CurrencyIdrConverter : IValueConverter
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        decimal amount = value is decimal d ? d : 0m;
        return string.Format(IdCulture, "Rp{0:N0}", amount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class AccountRoleDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var role = value?.ToString() ?? string.Empty;
        return role == "Default" ? "-" : ChartOfAccountsConstants.GetAccountRoleLabel(role);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is bool isActive && !isActive ? 0.5 : 1.0;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is bool isActive && isActive ? "Aktif" : "Nonaktif";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class StatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isActive = value is bool b && b;
        return isActive
            ? new SolidColorBrush(Colors.Transparent) // outline
            : new SolidColorBrush(Color.FromArgb(30, 128, 128, 128)); // secondary
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class AccountTypeToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value?.ToString()) switch
        {
            "Assets" => new SolidColorBrush(Color.FromArgb(40, 0, 120, 212)),         // Biru (Primary)
            "Liabilities" => new SolidColorBrush(Color.FromArgb(40, 196, 43, 28)),     // Merah (Destructive)
            "Equity" => new SolidColorBrush(Color.FromArgb(40, 135, 100, 184)),        // Ungu (Secondary)
            "OperatingIncome" or "OtherIncome" => new SolidColorBrush(Color.FromArgb(40, 16, 124, 16)), // Hijau
            "OperatingExpenses" or "OtherExpenses" => new SolidColorBrush(Color.FromArgb(40, 202, 80, 16)), // Oranye
            _ => new SolidColorBrush(Color.FromArgb(30, 128, 128, 128))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class AccountTypeToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value?.ToString()) switch
        {
            "Assets" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),
            "Liabilities" => new SolidColorBrush(Color.FromArgb(255, 196, 43, 28)),
            "Equity" => new SolidColorBrush(Color.FromArgb(255, 135, 100, 184)),
            "OperatingIncome" or "OtherIncome" => new SolidColorBrush(Color.FromArgb(255, 16, 124, 16)),
            "OperatingExpenses" or "OtherExpenses" => new SolidColorBrush(Color.FromArgb(255, 202, 80, 16)),
            _ => Application.Current.Resources["TextFillColorPrimaryBrush"] as Brush ?? new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

#endregion