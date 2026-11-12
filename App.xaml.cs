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

        // 1. Konfigurasi HttpClient ke backend Render
        services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://aumo.onrender.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Catatan: Render (terutama paket free tier) butuh waktu ~30-50 detik untuk cold start
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // 2. Daftarkan ViewModels (Transient: instance baru setiap kali halaman dibuka)
        // services.AddTransient<DashboardViewModel>();
        // services.AddTransient<LoginViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}