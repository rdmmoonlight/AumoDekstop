using System;
using System.Net.Http;
using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.ViewModels;
using AumoDekstop.Features.ChartOfAccounts.Services;
using AumoDekstop.Features.ChartOfAccounts.Views;
using AumoDekstop.Features.JournalEntry.Services;
using AumoDekstop.Features.JournalEntry.ViewModels;
using AumoDekstop.Features.Periods.Services;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Features.Reports.FinancialStatements.Services;
using AumoDekstop.Features.Reports.FinancialStatements.ViewModels;
using AumoDekstop.Features.Reports.FinancialStatements.Views;
using AumoDekstop.Features.Reports.TrialBalances.Services;
using AumoDekstop.Features.Reports.TrialBalances.ViewModels;
using AumoDekstop.Features.Reports.TrialBalances.Views;
using AumoDekstop.Features.Reports.Worksheet;
using AumoDekstop.Features.Shared.Http;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;
using Microsoft.Extensions.DependencyInjection;

namespace AumoDekstop.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAumoServices(this IServiceCollection services)
    {
        // 0. Penyimpanan token terpusat & handler autentikasi HTTP
        services.AddSingleton<ITokenStore>(_ => TokenStore.Instance);
        services.AddTransient<AuthHeaderHandler>();

        // 1. Named HttpClient untuk backend Render (tanpa ColdStartRetryHandler, retry dikelola ApiService)
        services.AddHttpClient("AumoApi", client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"AumoDekstop/{GitHubUpdateService.GetCurrentVersionString()}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Platform", "Windows-Desktop");

            // Timeout per-request cukup 30-45 detik karena retry loop dikelola di level ApiService
            client.Timeout = TimeSpan.FromSeconds(45);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        // 2. Satu-satunya pintu akses HTTP (IApiService & ApiService)
        services.AddHttpClient<IApiService, ApiService>("AumoApi");

        // Opsional: forward sementara jika masih ada kelas lama yang meminta concrete ApiService
        services.AddTransient<ApiService>(sp => (ApiService)sp.GetRequiredService<IApiService>());

        // 3. Autentikasi & Google OAuth
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGoogleAuthProvider, DesktopGoogleAuthProvider>();

        // 4. Periods
        services.AddSingleton<IPeriodsService, PeriodsService>();

        // 5. Chart Of Accounts
        services.AddSingleton<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddTransient<ChartOfAccountsPage>();

        // 6. Journal Entry
        services.AddSingleton<IJournalEntryService, JournalEntryService>();
        services.AddTransient<JournalEntryFormViewModel>();

        // 7. Auth & Periods ViewModels
        services.AddTransient<GoogleSignInViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<PeriodsPageViewModel>();

        // 8. Trial Balances & Worksheet
        services.AddTransient<ITrialBalancesApiService, TrialBalancesApiService>();
        services.AddSingleton<IWorksheetService, WorksheetService>();
        services.AddTransient<WorksheetPageViewModel>();
        services.AddTransient<WorksheetPage>();
        services.AddTransient<UnadjustedTrialBalanceViewModel>();
        services.AddTransient<AdjustedTrialBalanceViewModel>();
        services.AddTransient<PostClosingTrialBalanceViewModel>();
        services.AddTransient<UnadjustedTrialBalancePage>();
        services.AddTransient<AdjustedTrialBalancePage>();
        services.AddTransient<PostClosingTrialBalancePage>();

        // 9. Financial Statements
        services.AddTransient<IFinancialStatementsApiService, FinancialStatementsApiService>();
        services.AddTransient<IncomeStatementViewModel>();
        services.AddTransient<RetainedEarningsViewModel>();
        services.AddTransient<StatementOfCashFlowViewModel>();
        services.AddTransient<StatementOfFinancialPositionViewModel>();
        services.AddTransient<IncomeStatementPage>();
        services.AddTransient<RetainedEarningsPage>();
        services.AddTransient<StatementOfCashFlowPage>();
        services.AddTransient<StatementOfFinancialPositionPage>();

        return services;
    }
}