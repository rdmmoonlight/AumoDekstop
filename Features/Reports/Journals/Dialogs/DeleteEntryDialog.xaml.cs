
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace AumoDekstop.Features.Reports.Journals;
public sealed partial class DeleteEntryDialog : ContentDialog
{
    private readonly Run _transactionNumberRun = new();
    private readonly StackPanel _deletingStatusPanel = new()
    {
        Visibility = Visibility.Collapsed
    };

    public static readonly DependencyProperty TransactionNumberProperty =
        DependencyProperty.Register(
            nameof(TransactionNumber),
            typeof(string),
            typeof(DeleteEntryDialog),
            new PropertyMetadata(string.Empty, OnTransactionNumberChanged));

    public string TransactionNumber
    {
        get => (string)GetValue(TransactionNumberProperty);
        set => SetValue(TransactionNumberProperty, value);
    }

    /// <summary>
    /// Callback async yang dijalankan saat tombol Delete ditekan.
    /// Dialog akan otomatis terkunci (disabled) sampai Task selesai.
    /// </summary>
    public Func<Task>? ConfirmAsync { get; set; }

    public DeleteEntryDialog()
    {
        Title = "Delete entry?";
        PrimaryButtonText = "Delete";
        SecondaryButtonText = "Cancel";
        PrimaryButtonClick += OnPrimaryButtonClick;

        var message = new TextBlock { TextWrapping = TextWrapping.Wrap };
        message.Inlines.Add(new Run { Text = "Are you sure you want to delete entry " });
        message.Inlines.Add(_transactionNumberRun);
        message.Inlines.Add(new Run { Text = "? This action cannot be undone." });

        _deletingStatusPanel.Children.Add(new TextBlock { Text = "Deleting..." });
        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(message);
        content.Children.Add(_deletingStatusPanel);
        Content = content;

        UpdateTransactionNumber(TransactionNumber);
    }

    private static void OnTransactionNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DeleteEntryDialog dialog && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763, 0))
        {
            var number = e.NewValue as string ?? string.Empty;
            dialog.UpdateTransactionNumber(number);
        }
    }

    private void UpdateTransactionNumber(string number)
    {
        _transactionNumberRun.Text = $"\"{number}\"";
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (ConfirmAsync is null)
        {
            return;
        }

        // Tahan dialog agar tidak langsung tertutup sebelum proses hapus selesai
        var deferral = args.GetDeferral();

        try
        {
            SetDeletingState(true);
            await ConfirmAsync.Invoke();
        }
        finally
        {
            SetDeletingState(false);
            deferral.Complete();
        }
    }

    private void SetDeletingState(bool isDeleting)
    {
        IsPrimaryButtonEnabled = !isDeleting;
        IsSecondaryButtonEnabled = !isDeleting;
        PrimaryButtonText = isDeleting ? "Deleting..." : "Delete";
        _deletingStatusPanel.Visibility = isDeleting ? Visibility.Visible : Visibility.Collapsed;
    }
}