using AumoDekstop.Helpers;
using AumoDekstop.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace AumoDekstop.Views.Auth;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }
    private string? _redirectPath;

    public event EventHandler? LoginSucceeded;

    public LoginPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
        ViewModel.LoginSucceeded += OnAuthSuccess;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Jika halaman dipanggil dengan parameter rute redirect (misal "/dashboard")
        if (e.Parameter is string redirectPath)
        {
            _redirectPath = redirectPath;
        }
    }

    // Menggantikan perilaku submit <form> saat pengguna menekan tombol Enter
    private void OnInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SubmitCommand.CanExecute(null))
        {
            e.Handled = true;
            ViewModel.SubmitCommand.Execute(null);
        }
    }

    private void OnGoogleLoginSucceeded(object? sender, EventArgs e)
    {
        ViewModel.NotifyLoginSuccess();
    }

    private void OnAuthSuccess(object? sender, EventArgs e)
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);

        // Jika LoginPage digunakan dalam Frame navigasi utama, arahkan otomatis ke halaman tujuan
        if (Frame is not null)
        {
            var targetPage = AuthHelpers.ResolvePostLoginPage(_redirectPath);
            Frame.Navigate(targetPage);
        }
    }
}