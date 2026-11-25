
using System;
using AumoDekstop.Extensions;
using AumoDekstop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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
        Services = new ServiceCollection()
            .AddAumoServices()
            .BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();

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