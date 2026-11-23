namespace AumoDekstop;

using System.Net;
using System.Net.Http;
using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.ViewModels;
using AumoDekstop.Features.ChartOfAccounts.Services;
using AumoDekstop.Features.ChartOfAccounts.Views;
using AumoDekstop.Features.JournalEntry.Services;
using AumoDekstop.Features.JournalEntry.ViewModels;
using AumoDekstop.Features.Periods.Services;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Features.Reports.TrialBalances.Services;
using AumoDekstop.Features.Reports.TrialBalances.ViewModels;
using AumoDekstop.Features.Reports.TrialBalances.Views;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

public partial class App : Application
{
    private const string ThemeSettingKey = "AppRequestedTheme";
    private Window? _window;

    /// <summary>
    /// Menyediakan akses global ke ServiceProvider (Dependency Injection) di seluruh halaman/ViewModel.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Mengambil instance App yang sedang berjalan dengan tipe yang sudah di-cast.
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Mengambil tema yang sedang aktif saat ini (default: Dark).
    /// </summary>
    public ElementTheme CurrentTheme { get; private set; } = ElementTheme.Dark;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Named HttpClient untuk backend Render dengan CookieContainer persisten
        services.AddHttpClient("AumoApi", client =>
        {
            client.BaseAddress = new Uri(ApiService.BaseServerUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // Render (free tier) butuh ~30-50 detik untuk cold start
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        });

        // 2. ApiService singleton agar SetAuthToken() & Cookie bertahan di seluruh sesi
        services.AddSingleton<ApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ApiService(factory.CreateClient("AumoApi"));
        });

        // 3. Autentikasi & Google OAuth
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGoogleAuthProvider, DesktopGoogleAuthProvider>();

        // 4. Periods — WAJIB mengambil ApiService yang SAMA dari DI container (bukan ApiService.Instance)
        services.AddSingleton<IPeriodsApiService>(sp =>
            new PeriodsApiService(sp.GetRequiredService<ApiService>()));

        services.AddHttpClient<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddTransient<ChartOfAccountsPage>();

        // 5. Journal Entry Service (menggunakan named HttpClient "AumoApi")
        services.AddSingleton<IJournalEntryService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JournalEntryService(factory.CreateClient("AumoApi"));
        });

        // 6. ViewModels (Transient: instance baru setiap halaman/kontrol dibuka)
        services.AddTransient<GoogleSignInViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<PeriodsPageViewModel>();
        services.AddTransient<JournalEntryFormViewModel>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JournalEntryFormViewModel(
                factory.CreateClient("AumoApi"),
                sp.GetRequiredService<IJournalEntryService>());
        });

        // Service
        services.AddTransient<ITrialBalancesApiService, TrialBalancesApiService>();

        // ViewModels
        services.AddTransient<UnadjustedTrialBalanceViewModel>();
        services.AddTransient<AdjustedTrialBalanceViewModel>();
        services.AddTransient<PostClosingTrialBalanceViewModel>();

        // Pages
        services.AddTransient<UnadjustedTrialBalancePage>();
        services.AddTransient<AdjustedTrialBalancePage>();
        services.AddTransient<PostClosingTrialBalancePage>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();

        // Terapkan tema tersimpan, atau gunakan ElementTheme.Dark sebagai default
        var savedTheme = GetSavedTheme();
        SetTheme(savedTheme);

        _window.Activate();

        // Bangunkan server Render sejak aplikasi dibuka (cold start), tanpa menunggu hasilnya
        _ = Services.GetRequiredService<ApiService>().CheckServerHealthAsync("");
    }

    /// <summary>
    /// Mengubah tema aplikasi secara dinamis (Default: Dark) dan menyimpannya secara lokal.
    /// </summary>
    public void SetTheme(ElementTheme theme)
    {
        CurrentTheme = theme;

        if (_window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }

        AppLocalSettings.SetString(ThemeSettingKey, theme.ToString());
    }

    /// <summary>
    /// Membaca preferensi tema dari LocalSettings. Jika belum pernah diatur, kembalikan Dark.
    /// </summary>
    private static ElementTheme GetSavedTheme()
    {
        var themeName = AppLocalSettings.GetString(ThemeSettingKey);
        if (themeName is not null && Enum.TryParse<ElementTheme>(themeName, out var parsedTheme))
        {
            return parsedTheme;
        }

        return ElementTheme.Dark;
    }
}