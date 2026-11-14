namespace AumoDekstop.Features.Auth.Services;

public interface IGoogleAuthProvider
{
    /// <summary>
    /// Membuka alur login Google dan mengembalikan Google ID Token (JWT).
    /// Mengembalikan null jika dibatalkan oleh pengguna.
    /// </summary>
    Task<string?> SignInAndGetIdTokenAsync(string clientId, CancellationToken cancellationToken = default);
}