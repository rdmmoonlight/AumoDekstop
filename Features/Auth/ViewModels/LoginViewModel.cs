
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
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string Password { get; set; } = string.Empty;

    // Properti yang sebelumnya hilang dan menyebabkan XAML Compiler crash
    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotPending))]
    [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial bool IsPending { get; set; }
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
                ErrorMessage = string.IsNullOrWhiteSpace(res.Message)
                    ? "Email atau kata sandi salah."
                    : res.Message;
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
