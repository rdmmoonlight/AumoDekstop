using CommunityToolkit.Mvvm.ComponentModel;

namespace AumoDekstop.ViewModels.Auth;

public partial class RegisterViewModel : ObservableObject
{
    public string InitialMode { get; set; } = "register";

    // Pengganti defineEmits<{ (e: 'success'): void }>()
    public event EventHandler? RegisterSucceeded;

    public void NotifyRegisterSuccess()
    {
        RegisterSucceeded?.Invoke(this, EventArgs.Empty);
    }
}