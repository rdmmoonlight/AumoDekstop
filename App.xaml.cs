using AumoDekstop.Features.Auth.Services;
using AumoDekstop.Features.Auth.ViewModels;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace AumoDekstop.App;

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
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Konfigurasi Named HttpClient untuk backend Render
        services.AddHttpClient("AumoApi", client =>
        {
            client.BaseAddress = new Uri(ApiService.BaseServerUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // Catatan: Render (terutama paket free tier) butuh waktu ~30-50 detik untuk cold start
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // 2. Daftarkan ApiService sebagai Singleton agar SetAuthToken() bertahan di seluruh sesi
        services.AddSingleton<ApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ApiService(factory.CreateClient("AumoApi"));
        });

        // 3. Daftarkan Layanan Autentikasi & Google OAuth Provider
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGoogleAuthProvider, DesktopGoogleAuthProvider>();

        // 4. Daftarkan ViewModels (Transient: instance baru setiap kali halaman/kontrol dibuka)
        // services.AddTransient<DashboardViewModel>();
        services.AddTransient<GoogleSignInViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}