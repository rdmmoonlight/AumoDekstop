using AumoDekstop.Features.ChartOfAccounts.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Features.ChartOfAccounts.Dialogs;

public sealed partial class AccountFormDialog : ContentDialog
{
    private readonly Account? _editingAccount;

    /// <summary>
    /// Berisi hasil validasi form setelah pengguna menekan tombol Primary.
    /// </summary>
    public AccountFormValues? Result { get; private set; }

    public AccountFormDialog(XamlRoot xamlRoot, Account? editingAccount = null)
    {
        this.InitializeComponent();
        this.XamlRoot = xamlRoot;
        _editingAccount = editingAccount;

        TypeComboBox.ItemsSource = ChartOfAccountsConstants.AccountTypeOptions;
        RoleComboBox.ItemsSource = ChartOfAccountsConstants.AccountRoleOptions;

        if (_editingAccount != null)
        {
            Title = "Edit Akun";
            PrimaryButtonText = "Simpan Perubahan";
            IsActiveContainer.Visibility = Visibility.Visible;

            if (Enum.TryParse<AccountType>(_editingAccount.Type, true, out var parsedType))
            {
                TypeComboBox.SelectedValue = parsedType;
            }
            else
            {
                TypeComboBox.SelectedValue = AccountType.Assets;
            }

            if (Enum.TryParse<AccountRole>(_editingAccount.Role, true, out var parsedRole))
            {
                RoleComboBox.SelectedValue = parsedRole;
            }
            else
            {
                RoleComboBox.SelectedValue = AccountRole.Default;
            }

            ReferenceNumberBox.Text = _editingAccount.ReferenceNumber.ToString();
            AccountNameBox.Text = _editingAccount.AccountName;
            IsActiveSwitch.IsOn = _editingAccount.IsActive;
        }
        else
        {
            Title = "Tambah Akun Baru";
            PrimaryButtonText = "Tambah Akun";
            IsActiveContainer.Visibility = Visibility.Collapsed;

            TypeComboBox.SelectedValue = AccountType.Assets;
            RoleComboBox.SelectedValue = AccountRole.Default;
            IsActiveSwitch.IsOn = true;
        }

        UpdateReferencePlaceholder();
    }

    private AccountTypeOption? GetSelectedTypeOption()
    {
        return TypeComboBox.SelectedItem as AccountTypeOption;
    }

    private void UpdateReferencePlaceholder()
    {
        var opt = GetSelectedTypeOption();
        ReferenceNumberBox.PlaceholderText = opt != null
            ? $"Contoh: {opt.MinRange}"
            : "Contoh: 100";
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateReferencePlaceholder();
        if (!string.IsNullOrWhiteSpace(ReferenceNumberBox.Text))
        {
            ValidateReferenceNumber();
        }
    }

    private void ReferenceNumberBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ReferenceErrorText.Visibility == Visibility.Visible)
        {
            ValidateReferenceNumber();
        }
    }

    private void AccountNameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (NameErrorText.Visibility == Visibility.Visible)
        {
            ValidateAccountName();
        }
    }

    private bool ValidateAll()
    {
        bool isTypeValid = ValidateType();
        bool isRefValid = ValidateReferenceNumber();
        bool isNameValid = ValidateAccountName();
        bool isRoleValid = ValidateRole();

        return isTypeValid && isRefValid && isNameValid && isRoleValid;
    }

    private bool ValidateType()
    {
        if (TypeComboBox.SelectedValue == null)
        {
            ShowError(TypeErrorText, "Tipe akun wajib dipilih");
            return false;
        }
        HideError(TypeErrorText);
        return true;
    }

    private bool ValidateReferenceNumber()
    {
        var raw = ReferenceNumberBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(raw))
        {
            ShowError(ReferenceErrorText, "Kode akun wajib diisi");
            return false;
        }

        if (!int.TryParse(raw, out int number))
        {
            ShowError(ReferenceErrorText, "Kode akun harus berupa angka bulat");
            return false;
        }

        var opt = GetSelectedTypeOption();
        if (opt != null && (number < opt.MinRange || number > opt.MaxRange))
        {
            ShowError(ReferenceErrorText, $"Kode akun untuk tipe ini harus antara {opt.MinRange} dan {opt.MaxRange}");
            return false;
        }

        HideError(ReferenceErrorText);
        return true;
    }

    private bool ValidateAccountName()
    {
        if (string.IsNullOrWhiteSpace(AccountNameBox.Text))
        {
            ShowError(NameErrorText, "Nama akun wajib diisi");
            return false;
        }
        HideError(NameErrorText);
        return true;
    }

    private bool ValidateRole()
    {
        if (RoleComboBox.SelectedValue == null)
        {
            ShowError(RoleErrorText, "Peran akun wajib dipilih");
            return false;
        }
        HideError(RoleErrorText);
        return true;
    }

    private static void ShowError(TextBlock target, string message)
    {
        target.Text = message;
        target.Visibility = Visibility.Visible;
    }

    private static void HideError(TextBlock target)
    {
        target.Text = string.Empty;
        target.Visibility = Visibility.Collapsed;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Batalkan penutupan dialog jika validasi gagal
        if (!ValidateAll())
        {
            args.Cancel = true;
            return;
        }

        Result = new AccountFormValues
        {
            Type = (AccountType)TypeComboBox.SelectedValue!,
            ReferenceNumber = int.Parse(ReferenceNumberBox.Text.Trim()),
            AccountName = AccountNameBox.Text.Trim(),
            Role = (AccountRole)RoleComboBox.SelectedValue!,
            IsActive = _editingAccount != null ? IsActiveSwitch.IsOn : true
        };
    }
}