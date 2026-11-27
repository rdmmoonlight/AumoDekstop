using AumoDekstop.Features.ChartOfAccounts.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.ChartOfAccounts.Views;

public sealed partial class ChartOfAccountsPage : Page
{
    public ChartOfAccountsViewModel ViewModel { get; }

    public ChartOfAccountsPage()
        : this(App.Current.Services.GetRequiredService<ChartOfAccountsViewModel>())
    {
    }

    public ChartOfAccountsPage(ChartOfAccountsViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAccountsCommand.ExecuteAsync(null);
    }

    private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        await AddAccountAsync();
    }

    /// <summary>Memuat ulang daftar akun (dipakai tombol ribbon).</summary>
    public Task RefreshAsync() => ViewModel.LoadAccountsCommand.ExecuteAsync(null);

    /// <summary>Membuka form tambah akun (dipakai tombol ribbon).</summary>
    public async Task AddAccountAsync()
    {
        if (ViewModel.IsSubmitting)
        {
            return;
        }

        ViewModel.ClearError();

        var dialog = new AccountFormDialog(this.XamlRoot, editingAccount: null);
        var dialogResult = await dialog.ShowAsync();

        if (dialogResult != ContentDialogResult.Primary || dialog.Result is null)
        {
            return;
        }

        await ViewModel.CreateAccountAsync(dialog.Result);
    }

    private async void AccountsTable_EditAccountRequested(object? sender, Account account)
    {
        await EditAccountAsync(account);
    }

    /// <summary>Mengubah akun yang dipilih di tabel. False jika belum ada baris terpilih.</summary>
    public async Task<bool> EditSelectedAsync()
    {
        var account = ViewModel.SelectedAccount ?? AccountsTable.SelectedAccount;
        if (account is null)
        {
            return false;
        }

        await EditAccountAsync(account);
        return true;
    }

    /// <summary>Menghapus akun yang dipilih di tabel. False jika belum ada baris terpilih.</summary>
    public async Task<bool> DeleteSelectedAsync()
    {
        var account = ViewModel.SelectedAccount ?? AccountsTable.SelectedAccount;
        if (account is null)
        {
            return false;
        }

        await DeleteAccountAsync(account);
        return true;
    }

    private async Task EditAccountAsync(Account account)
    {
        if (ViewModel.IsSubmitting)
        {
            return;
        }

        ViewModel.ClearError();

        var dialog = new AccountFormDialog(this.XamlRoot, editingAccount: account);
        var dialogResult = await dialog.ShowAsync();

        if (dialogResult != ContentDialogResult.Primary || dialog.Result is null)
        {
            return;
        }

        await ViewModel.UpdateAccountAsync(account.Id, dialog.Result);
    }

    private async void AccountsTable_DeleteAccountRequested(object? sender, Account account)
    {
        await DeleteAccountAsync(account);
    }

    private async Task DeleteAccountAsync(Account account)
    {
        if (ViewModel.IsSubmitting)
        {
            return;
        }

        ViewModel.ClearError();

        bool confirmed = await DeleteAccountDialog.ShowAsync(this.XamlRoot, account);
        if (!confirmed)
        {
            return;
        }

        await ViewModel.DeleteAccountAsync(account.Id);
    }

    private void ErrorInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        ViewModel.ClearError();
    }
}