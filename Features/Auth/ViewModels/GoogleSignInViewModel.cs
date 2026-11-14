using AumoDekstop.Features.Auth.Helpers;
using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Features.Auth.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AumoDekstop.Features.Auth.ViewModels;

public partial class GoogleSignInViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthProvider _googleAuthProvider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInWithGoogleCommand))]
    private bool _isLoading;

    public bool HasClientId => !string.IsNullOrWhiteSpace(AuthHelpers.GoogleClientId);
    public bool HasNoClientId => !HasClientId;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    // Pengganti defineEmits<{ (e: 'success'): void }>()
    public event EventHandler? LoginSucceeded;

    public GoogleSignInViewModel(IAuthService authService, IGoogleAuthProvider googleAuthProvider)
    {
        _authService = authService;
        _googleAuthProvider = googleAuthProvider;
    }

    [RelayCommand(CanExecute = nameof(CanSignInWithGoogle))]
    private async Task SignInWithGoogleAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var idToken = await _googleAuthProvider.SignInAndGetIdTokenAsync(AuthHelpers.GoogleClientId);

            if (string.IsNullOrWhiteSpace(idToken))
            {
                ErrorMessage = "Token Google tidak diterima.";
                return;
            }

            var res = await ((AuthService)_authService).GoogleLoginAsync(
                new GoogleLoginRequest(idToken), CancellationToken.None);

            if (res is not { Success: true })
            {
                ErrorMessage = "Login Google gagal.";
                return;
            }

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // Catch tetap dipertahankan khusus untuk error dari _googleAuthProvider (browser/OAuth lokal)
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSignInWithGoogle() => HasClientId && !IsLoading;
}