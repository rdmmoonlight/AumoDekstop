using System;
using System.Threading.Tasks;
using AumoDekstop.Features.ChartOfAccounts.Views;
using AumoDekstop.Features.Periods.Dialogs;
using AumoDekstop.Features.Periods.Views;
using AumoDekstop.Features.Reports.FinancialStatements.Views;
using AumoDekstop.Features.Reports.TrialBalances.Views;
using AumoDekstop.Features.Reports.Worksheet;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;
using AumoDekstop.Shell.Ribbon;

namespace AumoDekstop;

public sealed partial class MainWindow
{
    // Satu pintu untuk semua tombol ribbon (lihat RibbonCommand)
    private async void OnRibbonCommand(object? sender, string command)
    {
        switch (command)
        {
            case RibbonCommand.New:
                await NewAsync();
                break;
            case RibbonCommand.Save:
                await SaveAsync();
                break;
            case RibbonCommand.Sync:
                await SyncAsync();
                break;
            case RibbonCommand.CheckUpdate:
                await _updates.CheckAndPromptAsync(Content?.XamlRoot, isManualCheck: true);
                break;
            case RibbonCommand.RefreshPeriods:
                await RefreshPeriodsAsync();
                break;
            case RibbonCommand.RefreshCoa:
                await RunCoaCommandAsync("● Memuat ulang daftar akun (COA)...", async page =>
                {
                    await page.RefreshAsync();
                    SetStatus("● Daftar akun (COA) diperbarui.");
                });
                break;
            case RibbonCommand.ImportCoa:
                SetStatus("● Impor COA belum tersedia.");
                break;
            case RibbonCommand.AddCoa:
                await RunCoaCommandAsync("● Membuka form tambah akun (COA)...", page => page.AddAccountAsync());
                break;
            case RibbonCommand.EditCoa:
                await RunCoaCommandAsync("● Membuka form ubah akun (COA)...", async page =>
                {
                    if (!await page.EditSelectedAsync())
                    {
                        SetStatus("● Pilih satu akun pada tabel terlebih dahulu.");
                    }
                });
                break;
            case RibbonCommand.DeleteCoa:
                await RunCoaCommandAsync("● Menghapus akun (COA) terpilih...", async page =>
                {
                    if (!await page.DeleteSelectedAsync())
                    {
                        SetStatus("● Pilih satu akun pada tabel terlebih dahulu.");
                    }
                });
                break;

            // Trial Balances (Neraca Saldo)
            case RibbonCommand.UnadjustedTrialBalance:
                await ShowReportPageAsync<UnadjustedTrialBalancePage>(
                    "Unadjusted Trial Balance",
                    p => p.ViewModel.LoadAsync());
                break;
            case RibbonCommand.AdjustedTrialBalance:
                await ShowReportPageAsync<AdjustedTrialBalancePage>(
                    "Adjusted Trial Balance",
                    p => p.ViewModel.LoadAsync());
                break;
            case RibbonCommand.PostClosingTrialBalance:
                await ShowReportPageAsync<PostClosingTrialBalancePage>(
                    "Post-Closing Trial Balance",
                    p => p.ViewModel.LoadAsync());
                break;

            case RibbonCommand.Worksheet:
                await ShowReportPageAsync<WorksheetPage>(
                    "Worksheet",
                    p => p.ViewModel.LoadAsync());
                break;

            // Financial Statements (Laporan Keuangan)
            case RibbonCommand.IncomeStatement:
                await ShowReportPageAsync<IncomeStatementPage>(
                    "Laporan Laba Rugi",
                    p => p.ViewModel.LoadAsync());
                break;
            case RibbonCommand.RetainedEarnings:
                await ShowReportPageAsync<RetainedEarningsPage>(
                    "Laporan Laba Ditahan",
                    p => p.ViewModel.LoadAsync());
                break;
            case RibbonCommand.CashFlow:
                await ShowReportPageAsync<StatementOfCashFlowPage>(
                    "Laporan Arus Kas",
                    p => p.ViewModel.LoadAsync());
                break;
            case RibbonCommand.FinancialPosition:
                await ShowReportPageAsync<StatementOfFinancialPositionPage>(
                    "Laporan Posisi Keuangan (Neraca)",
                    p => p.ViewModel.LoadAsync());
                break;

            case RibbonCommand.Logout:
                await LogoutAsync();
                break;
            case RibbonCommand.ComingSoon:
                SetStatus("● Fitur ini belum tersedia.");
                break;
        }
    }

    // Perintah COA hanya berlaku saat halaman Chart Of Accounts sedang tampil
    private async Task RunCoaCommandAsync(string statusText, Func<ChartOfAccountsPage, Task> action)
    {
        if (RootFrame.Content is not ChartOfAccountsPage page)
        {
            return;
        }

        SetStatus(statusText);
        await action(page);
    }

    private async Task RefreshPeriodsAsync()
    {
        if (RootFrame.Content is PeriodsPage periodsPage)
        {
            SetStatus("● Memuat ulang daftar periode...");
            await periodsPage.ViewModel.FetchPeriodsCommand.ExecuteAsync(null);
            SetStatus("● Daftar periode diperbarui.");
        }
    }

    private async Task NewAsync()
    {
        // Di halaman Periods, tombol "Baru" membuka dialog Buka Periode Baru
        if (RootFrame.Content is PeriodsPage periodsPage && Content?.XamlRoot != null)
        {
            SetStatus("● Membuka form periode baru...");

            var dialog = new OpenNewPeriodDialog
            {
                XamlRoot = Content.XamlRoot,
                OnSubmitAsync = async payload =>
                {
                    await periodsPage.ViewModel.CreatePeriodCommand.ExecuteAsync(payload);
                }
            };

            await dialog.ShowAsync();
            return;
        }

        SetStatus("● Membuat entri baru...");
    }

    private async Task SyncAsync()
    {
        SetStatus($"● Menghubungi server {ApiConfig.BaseUrl} (mohon tunggu)...");

        bool isOnline = await _api.CheckServerHealthAsync("api/v1/health");

        if (isOnline)
        {
            if (RootFrame.Content is PeriodsPage periodsPage)
            {
                await periodsPage.ViewModel.FetchPeriodsCommand.ExecuteAsync(null);
            }

            SetStatus($"● Terhubung ke Server ({ApiConfig.BaseUrl}) | Versi: v{GitHubUpdateService.GetCurrentVersionString()}");
        }
        else
        {
            SetStatus("● Server tidak merespons atau masih dalam proses bangun (cold start).");
        }
    }

    private async Task SaveAsync()
    {
        SetStatus("● Mengirim data ke server...");

        var payload = new
        {
            Client = "AumoDekstop",
            Timestamp = DateTime.UtcNow
        };

        var result = await _api.PostAsync<object, object>("api/data", payload);

        SetStatus(result.IsSuccess
            ? "● Data berhasil disimpan ke server."
            : $"● Gagal menyimpan: {result.ErrorMessage}");
    }
}
