using AumoDekstop.Features.Auth.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.Auth.Views;

public sealed partial class GoogleSignInControl : UserControl
{
    public GoogleSignInViewModel ViewModel { get; }

    // Event XAML agar halaman induk bisa menulis: <controls:GoogleSignInControl LoginSucceeded="OnGoogleSuccess" />
    public event EventHandler? LoginSucceeded;

    public GoogleSignInControl()
    {
        ViewModel = App.Current.Services.GetRequiredService<GoogleSignInViewModel>();
        ViewModel.LoginSucceeded += (_, args) => LoginSucceeded?.Invoke(this, args);

        // Wajib dipanggil agar tampilan dan binding XAML dimuat
        InitializeComponent();
    }
}
