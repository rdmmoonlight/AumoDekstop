namespace AumoDekstop.Features.Auth.Services;

using System;
using System.Linq;
using AumoDekstop.Services;
using Windows.Security.Credentials;

public static class SessionStorageService
{
    private const string VaultResourceName = "AumoDekstopAuthToken";
    private const string RememberMeKey = "IsRememberMe";
    private const string SavedEmailKey = "SavedUserEmail";

    public static void SaveSession(string email, string authToken, bool rememberMe)
    {
        AppLocalSettings.SetBool(RememberMeKey, rememberMe);

        // Hapus kredensial lama jika ada
        ClearVault();

        if (rememberMe && !string.IsNullOrEmpty(authToken))
        {
            AppLocalSettings.SetString(SavedEmailKey, email);
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(VaultResourceName, email, authToken));
        }
        else
        {
            AppLocalSettings.Remove(SavedEmailKey);
        }
    }

    public static (string? Email, string? Token) GetSavedSession()
    {
        bool isRemembered = AppLocalSettings.GetBool(RememberMeKey, false);

        if (!isRemembered)
            return (null, null);

        try
        {
            var vault = new PasswordVault();
            var credential = vault.FindAllByResource(VaultResourceName).FirstOrDefault();
            if (credential != null)
            {
                credential.RetrievePassword();
                return (credential.UserName, credential.Password);
            }
        }
        catch
        {
            // Tidak ada kredensial ditemukan di vault
        }

        return (null, null);
    }

    public static void ClearSession()
    {
        AppLocalSettings.SetBool(RememberMeKey, false);
        AppLocalSettings.Remove(SavedEmailKey);
        ClearVault();
    }

    private static void ClearVault()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(VaultResourceName);
            foreach (var cred in credentials)
            {
                vault.Remove(cred);
            }
        }
        catch { /* Abaikan jika vault kosong */ }
    }
}
