using System;
using System.Net;
using System.Net.Http;
using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.ViewModels;
using AumoDekstop.Features.Periods.Services;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace AumoDekstop;

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

        // 5. ViewModels (Transient: instance baru setiap halaman/kontrol dibuka)
        services.AddTransient<GoogleSignInViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<PeriodsPageViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();

        // Terapkan tema tersimpan, atau gunakan ElementTheme.Dark sebagai default
        var savedTheme = GetSavedTheme();
        SetTheme(savedTheme);

        _window.Activate();
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

        ApplicationData.Current.LocalSettings.Values[ThemeSettingKey] = theme.ToString();
    }

    /// <summary>
    /// Membaca preferensi tema dari LocalSettings. Jika belum pernah diatur, kembalikan Dark.
    /// </summary>
    private static ElementTheme GetSavedTheme()
    {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(ThemeSettingKey, out var value)
            && value is string themeName
            && Enum.TryParse<ElementTheme>(themeName, out var parsedTheme))
        {
            return parsedTheme;
        }

        return ElementTheme.Dark;
    }
}
