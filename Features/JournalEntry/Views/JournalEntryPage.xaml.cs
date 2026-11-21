namespace AumoDekstop.Features.JournalEntry.Views;

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AumoDekstop.Features.JournalEntry.Controls;
using AumoDekstop.Features.JournalEntry.Models;
using AumoDekstop.Features.JournalEntry.Services;
using AumoDekstop.Features.JournalEntry.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class JournalEntryPage : Page, INotifyPropertyChanged
{
    private readonly IJournalEntryService _journalEntryService;

    private string? _entryId;
    private bool _isSubmitting;
    private string? _errorMessage;
    private string _entryStatus = "DRAFT";

    public event PropertyChangedEventHandler? PropertyChanged;

    public JournalEntryFormViewModel ViewModel { get; }

    public JournalEntryPage()
    {
        InitializeComponent();

        // Ambil instance dari DI container aplikasi (sesuaikan dengan setup App.Services Anda)
        _journalEntryService = App.Current.Services.GetRequiredService<IJournalEntryService>();
        ViewModel = App.Current.Services.GetRequiredService<JournalEntryFormViewModel>();

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.Lines.CollectionChanged += Lines_CollectionChanged;
    }

    #region State Properties (Pengganti ref & computed di Vue)

    public string? EntryId
    {
        get => _entryId;
        private set
        {
            if (SetField(ref _entryId, value))
            {
                OnPropertyChanged(nameof(IsEditMode));
            }
        }
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        private set => SetField(ref _isSubmitting, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    public string EntryStatus
    {
        get => _entryStatus;
        private set
        {
            if (SetField(ref _entryStatus, value))
            {
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }
    }

    public bool IsEditMode => !string.IsNullOrWhiteSpace(EntryId);

    public bool IsReadOnly => EntryStatus != "DRAFT";

    public bool HasLines => ViewModel.Lines.Count > 0;

    #endregion

    #region Navigation & Lifecycle (Pengganti useRoute & onMounted)

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Tangkap parameter id jika dikirim saat navigasi (misal: Frame.Navigate(typeof(JournalEntryPage), "123"))
        EntryId = e.Parameter?.ToString();

        await ViewModel.InitializeAsync(EntryId);

        // Gabungkan error awal jika gagal load akun atau detail
        ErrorMessage = ViewModel.DetailError ?? ViewModel.AccountsError;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JournalEntryFormViewModel.Entry) && ViewModel.Entry is not null)
        {
            // Jika DTO memiliki properti Status, ambil dari DTO; default ke "DRAFT"
            EntryStatus = "DRAFT";
        }
    }

    private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasLines));
    }

    #endregion

    #region Event Handlers (Pengganti handleCancel, handleSaveDraft, handlePost)

    private void JournalLinesTable_LineUpdated(object? sender, JournalLineUpdatedEventArgs e)
    {
        ViewModel.UpdateLine(e.Index, e.Field, e.Value);
    }

    private void FormActions_CancelClicked(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void FormActions_SubmitClicked(object sender, RoutedEventArgs e)
    {
        await HandleSaveDraftAsync();
    }

    private async void FormActions_DeleteClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryId) || !int.TryParse(EntryId, out var numericId))
        {
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;
        try
        {
            await _journalEntryService.DeleteAsync(numericId);
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal menghapus entri jurnal";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public async Task HandleSaveDraftAsync()
    {
        IsSubmitting = true;
        ErrorMessage = null;
        try
        {
            var entryDate = DateTime.TryParseExact(
                ViewModel.TransactionDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate)
                ? parsedDate
                : DateTime.Today;

            var lineDtos = ViewModel.Lines.Select(l => new JournalEntryLineDto(
                AccountId: int.TryParse(l.AccountId, out var accId) ? accId : 0,
                Description: l.Description,
                Debit: l.Debit,
                Credit: l.Credit
            )).ToList();

            if (!string.IsNullOrWhiteSpace(EntryId) && int.TryParse(EntryId, out var numericId))
            {
                var updatePayload = new UpdateJournalEntryRequest(
                    TransactionNumber: ViewModel.ReferenceNumber,
                    JournalType: "GENERAL",
                    EntryDate: entryDate,
                    Description: ViewModel.Description,
                    Lines: lineDtos
                );

                await _journalEntryService.EditAsync(numericId, updatePayload);
                await ViewModel.FetchDetailAsync(EntryId);
            }
            else
            {
                var createPayload = new CreateJournalEntryRequest(
                    TransactionNumber: ViewModel.ReferenceNumber,
                    JournalType: "GENERAL",
                    EntryDate: entryDate,
                    Description: ViewModel.Description,
                    Lines: lineDtos
                );

                var created = await _journalEntryService.CreateAsync(createPayload);
                if (created is not null)
                {
                    EntryId = created.Id.ToString(CultureInfo.InvariantCulture);
                    await ViewModel.FetchDetailAsync(EntryId);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal menyimpan draf jurnal";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public async Task HandlePostAsync()
    {
        if (!ViewModel.IsBalanced) return;

        IsSubmitting = true;
        ErrorMessage = null;
        try
        {
            if (string.IsNullOrWhiteSpace(EntryId))
            {
                await HandleSaveDraftAsync();
            }

            if (!string.IsNullOrWhiteSpace(EntryId))
            {
                await ViewModel.FetchDetailAsync(EntryId);
                EntryStatus = "POSTED";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = !string.IsNullOrWhiteSpace(ex.Message)
                ? ex.Message
                : "Gagal melakukan posting jurnal";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    #endregion

    #region INotifyPropertyChanged Helpers

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
