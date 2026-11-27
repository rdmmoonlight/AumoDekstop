using System;
using System.Threading;
using System.Threading.Tasks;
using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Features.Shared.Http;
using AumoDekstop.Services;

namespace AumoDekstop.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ITokenStore _tokenStore;

    public UserProfileData? CachedProfile { get; private set; }
    public bool IsAuthenticated => _tokenStore.HasToken;

    public AuthService(IApiService apiService, ITokenStore? tokenStore = null)
    {
        ArgumentNullException.ThrowIfNull(apiService);
        _apiService = apiService;
        _tokenStore = tokenStore ?? TokenStore.Instance;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            if (string.IsNullOrWhiteSpace(authData.Token))
            {
                return new AuthResponse(false, "Server tidak mengirim token");
            }

            ApplyToken(authData.Token);
            SessionStorageService.SaveSession(request.Email, authData.Token, request.RememberMe);
            CachedProfile = null;
        }

        return ToAuthResponse(response);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<GoogleLoginRequest, AuthResponse>("api/v1/auth/google-login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            if (string.IsNullOrWhiteSpace(authData.Token))
            {
                return new AuthResponse(false, "Server tidak mengirim token");
            }

            ApplyToken(authData.Token);
            SessionStorageService.SaveSession(authData.UserId ?? "google-user", authData.Token, true);
            CachedProfile = null;
        }

        return ToAuthResponse(response);
    }

    public async Task<(bool IsRestored, string? ErrorMessage)> RestoreSessionAsync(CancellationToken ct = default)
    {
        // 1. GetSavedSession(). Bila kosong -> kembalikan false
        var (_, savedToken) = SessionStorageService.GetSavedSession();
        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return (false, null);
        }

        // 2. ITokenStore.Set(token)
        ApplyToken(savedToken);

        // 3. GET api/v1/auth/me
        var response = await _apiService.GetAsync<ProfileResponse>("api/v1/auth/me", ct);

        // Bila 200 -> isi CachedProfile, kembalikan true
        if (response.IsSuccess && response.Data is { Success: true, Data: not null } profileRes)
        {
            CachedProfile = profileRes.Data;
            return (true, null);
        }

        // 4. Bila 401 -> ClearSession() dan SessionStorageService.ClearSession(), kembalikan false
        if (response.StatusCode == 401)
        {
            ClearSession();
            SessionStorageService.ClearSession();
            return (false, null);
        }

        // 5. Bila galat jaringan (status 0) -> pertahankan token, kembalikan false + pesan
        if (response.StatusCode == 0)
        {
            return (false, "Server belum dapat dihubungi");
        }

        return (false, response.ErrorMessage);
    }

    public async Task<ProfileResponse> GetProfileAsync(bool forceRefresh = false, CancellationToken ct = default)
    {
        if (!forceRefresh && CachedProfile is not null)
        {
            return new ProfileResponse(true, CachedProfile);
        }

        var response = await _apiService.GetAsync<ProfileResponse>("api/v1/auth/me", ct);

        if (response.IsSuccess && response.Data is { Success: true, Data: not null } profileRes)
        {
            CachedProfile = profileRes.Data;
        }

        return response.Data ?? new ProfileResponse(false, null, response.ErrorMessage);
    }

    public async Task<CommonActionResponse> LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiService.PostEmptyAsync<CommonActionResponse>("api/v1/auth/logout", ct);
            return response.Data ?? new CommonActionResponse(response.IsSuccess, response.ErrorMessage ?? string.Empty);
        }
        finally
        {
            ClearSession();
            SessionStorageService.ClearSession();
        }
    }

    public void ClearSession()
    {
        CachedProfile = null;
        _tokenStore.Clear();
    }

    private static AuthResponse ToAuthResponse(ApiResponse<AuthResponse> response)
    {
        if (response.Data is not null)
        {
            return response.Data;
        }

        string message = response.StatusCode switch
        {
            400 or 401 => "Email atau kata sandi salah.",
            0 or >= 500 => response.ErrorMessage ?? "Server belum siap atau tidak merespons. Silakan coba lagi.",
            _ => response.ErrorMessage ?? "Permintaan ditolak oleh server."
        };

        return new AuthResponse(false, message);
    }

    private void ApplyToken(string token)
    {
        _tokenStore.Set(token);
    }
}