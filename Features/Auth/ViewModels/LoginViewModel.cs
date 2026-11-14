using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Features.Auth.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.Auth.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _password = string.Empty;

    // Properti yang sebelumnya hilang dan menyebabkan XAML Compiler crash
    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotPending))]
    [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _isPending;

    public string InitialMode { get; set; } = "login";

    public bool IsNotPending => !IsPending;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public string SubmitButtonText => IsPending ? "Memproses..." : "Masuk";

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    private bool CanSubmit() =>
        !IsPending &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        IsPending = true;

        try
        {
            // Panggil langsung melalui interface IAuthService tanpa casting ke AuthService
            // (Sesuaikan parameter LoginRequest jika mendukung RememberMe)
            var res = await _authService.LoginAsync(
                new LoginRequest(Email.Trim(), Password),
                cancellationToken);

            if (!res.Success)
            {
                ErrorMessage = "Email atau kata sandi salah.";
                return;
            }

            NotifyLoginSuccess();
        }
        catch (OperationCanceledException)
        {
            // Abaikan jika dibatalkan
        }
        catch (Exception)
        {
            ErrorMessage = "Terjadi kesalahan saat menghubungkan ke server. Silakan coba lagi.";
        }
        finally
        {
            IsPending = false;
        }
    }

    public void NotifyLoginSuccess()
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }
}
