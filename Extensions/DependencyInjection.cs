
using System;
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
using Microsoft.Extensions.DependencyInjection;

namespace AumoDekstop.Extensions;
public static class DependencyInjection
{
    public static IServiceCollection AddAumoServices(this IServiceCollection services)
    {
        // 0. Penyimpanan token terpusat & handler Bearer + 401 otomatis
        services.AddSingleton<ITokenStore>(_ => TokenStore.Instance);
        services.AddTransient<AuthHeaderHandler>();

        // 1. Named HttpClient untuk backend Render dengan AuthHeaderHandler
        services.AddHttpClient("AumoApi", client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        // 2. ApiService singleton
        services.AddSingleton<ApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ApiService(factory.CreateClient("AumoApi"));
        });

        // 3. Autentikasi & Google OAuth
        services.AddSingleton<IAuthService>(sp =>
            new AuthService(
                sp.GetRequiredService<ApiService>(),
                sp.GetRequiredService<ITokenStore>()));
        services.AddSingleton<IGoogleAuthProvider, DesktopGoogleAuthProvider>();

        // 4. Periods — WAJIB mengambil ApiService yang SAMA dari DI container (bukan ApiService.Instance)
        services.AddSingleton<IPeriodsApiService>(sp =>
            new PeriodsApiService(sp.GetRequiredService<ApiService>()));

        // 5. Chart Of Accounts
        services.AddSingleton<IChartOfAccountsService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ChartOfAccountsService(factory.CreateClient("AumoApi"));
        });
        services.AddTransient<ChartOfAccountsPage>();

        // 6. Journal Entry
        services.AddSingleton<IJournalEntryService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JournalEntryService(factory.CreateClient("AumoApi"));
        });
        services.AddTransient<JournalEntryFormViewModel>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JournalEntryFormViewModel(
                factory.CreateClient("AumoApi"),
                sp.GetRequiredService<IJournalEntryService>());
        });

        // 7. Auth & Periods ViewModels
        services.AddTransient<GoogleSignInViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<PeriodsPageViewModel>();

        // 8. Trial Balances & Worksheet
        services.AddTransient<ITrialBalancesApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new TrialBalancesApiService(factory.CreateClient("AumoApi"));
        });
        services.AddSingleton<IWorksheetService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new WorksheetService(factory.CreateClient("AumoApi"));
        });
        services.AddTransient<WorksheetPageViewModel>();
        services.AddTransient<WorksheetPage>();
        services.AddTransient<UnadjustedTrialBalanceViewModel>();
        services.AddTransient<AdjustedTrialBalanceViewModel>();
        services.AddTransient<PostClosingTrialBalanceViewModel>();
        services.AddTransient<UnadjustedTrialBalancePage>();
        services.AddTransient<AdjustedTrialBalancePage>();
        services.AddTransient<PostClosingTrialBalancePage>();

        // 9. Financial Statements
        services.AddTransient<IFinancialStatementsApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new FinancialStatementsApiService(factory.CreateClient("AumoApi"));
        });
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