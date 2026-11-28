using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AumoDekstop.Extensions;
using AumoDekstop.Services;
using AumoDekstop.Services.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

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
        AppLogger.Init();
        RegisterGlobalExceptionHandlers();

        InitializeComponent();
        Services = new ServiceCollection()
            .AddAumoServices()
            .BuildServiceProvider();
    }

    private void RegisterGlobalExceptionHandlers()
    {
        // 1. Menangkap crash pada UI Thread / XAML Binding / Navigation WinUI 3
        this.UnhandledException += (_, e) =>
        {
            var diagnosticReport = FormatExceptionDiagnostics(e.Exception, e.Message);
            AppLogger.Fatal("WinUI.UnhandledException", diagnosticReport, e.Exception);
        };

        // 2. Menangkap crash pada background thread / domain level
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            var diagnosticReport = ex is not null
                ? FormatExceptionDiagnostics(ex, $"IsTerminating={e.IsTerminating}")
                : $"IsTerminating={e.IsTerminating}: {e.ExceptionObject}";

            AppLogger.Fatal("AppDomain.UnhandledException", diagnosticReport, ex);
        };

        // 3. Menangkap exception dari Task async yang tidak di-await
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            var diagnosticReport = FormatExceptionDiagnostics(e.Exception, "Unobserved task exception");
            AppLogger.Error("TaskScheduler.Unobserved", diagnosticReport, e.Exception);
            e.SetObserved();
        };
    }

    /// <summary>
    /// Membongkar seluruh rantai InnerException, HResult, dan konteks runtime WinUI
    /// agar penyebab asli XamlParseException / DI error terlihat jelas di log.
    /// </summary>
    private static string FormatExceptionDiagnostics(Exception? rootException, string? fallbackMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.IsNullOrWhiteSpace(fallbackMessage) ? "Unhandled Exception" : fallbackMessage);
        sb.AppendLine($"  BaseDirectory : {AppContext.BaseDirectory}");
        sb.AppendLine($"  HasResourcesPri: {File.Exists(Path.Combine(AppContext.BaseDirectory, "resources.pri"))}");

        if (rootException is null)
        {
            return sb.ToString().TrimEnd();
        }

        var current = rootException;
        int depth = 0;

        while (current is not null)
        {
            string prefix = depth == 0 ? "RootException" : $"InnerException[{depth}]";
            sb.AppendLine($"  {prefix}: {current.GetType().FullName} (HResult: 0x{current.HResult:X8})");
            sb.AppendLine($"    Message: {current.Message}");

            if (current is XamlParseException xamlEx)
            {
                sb.AppendLine($"    XAML Location: Line {xamlEx.LineNumber}, Column {xamlEx.LinePosition}");
            }

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                sb.AppendLine("    StackTrace:");
                foreach (var line in current.StackTrace.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"      {line.Trim()}");
                }
            }

            current = current.InnerException;
            depth++;
        }

        return sb.ToString().TrimEnd();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();

        var savedTheme = GetSavedTheme();
        SetTheme(savedTheme);

        _window.Activate();

        // Bangunkan server Render sejak aplikasi dibuka (cold start), tanpa menunggu hasilnya
        _ = Services.GetRequiredService<ApiService>().CheckServerHealthAsync("api/v1/health");
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
