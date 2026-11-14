using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.ViewModels;
using AumoDekstop.Features.Periods.Services;
using AumoDekstop.Features.Periods.ViewModels;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace AumoDekstop;

public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// Menyediakan akses global ke ServiceProvider (Dependency Injection) di seluruh halaman/ViewModel.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Mengambil instance App yang sedang berjalan dengan tipe yang sudah di-cast.
    /// </summary>
    public new static App Current => (App)Application.Current;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Named HttpClient untuk backend Render
        services.AddHttpClient("AumoApi", client =>
        {
            client.BaseAddress = new Uri(ApiService.BaseServerUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // Render (free tier) butuh ~30-50 detik untuk cold start
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // 2. ApiService singleton agar SetAuthToken() bertahan di seluruh sesi
        services.AddSingleton<ApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ApiService(factory.CreateClient("AumoApi"));
        });

        // 3. Autentikasi & Google OAuth
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGoogleAuthProvider, DesktopGoogleAuthProvider>();

        // 4. Periods
        services.AddSingleton<IPeriodsApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new PeriodsApiService(factory.CreateClient("AumoApi"));
        });

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
        _window.Activate();
    }
}
