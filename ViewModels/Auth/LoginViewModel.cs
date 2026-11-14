using AumoDekstop.Models.Auth;
using AumoDekstop.Services.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _password = string.Empty;

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

    // Pengganti defineEmits<{ (e: 'success'): void }>()
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
    private async Task SubmitAsync()
    {
        ErrorMessage = string.Empty;
        IsPending = true;

        try
        {
            var res = await ((AuthService)_authService).LoginAsync(
                new LoginRequest(Email.Trim(), Password),
                System.Threading.CancellationToken.None);

            if (!res.Success)
            {
                ErrorMessage = "Email atau kata sandi salah.";
                return;
            }

            NotifyLoginSuccess();
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