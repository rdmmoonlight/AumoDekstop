using AumoDekstop.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Controls.Auth;

public sealed partial class GoogleSignInControl : UserControl
{
    public GoogleSignInViewModel ViewModel { get; }

    // Event XAML agar halaman induk bisa menulis: <controls:GoogleSignInControl LoginSucceeded="OnGoogleSuccess" />
    public event EventHandler? LoginSucceeded;

    public GoogleSignInControl()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<GoogleSignInViewModel>();
        ViewModel.LoginSucceeded += (_, args) => LoginSucceeded?.Invoke(this, args);
    }
}