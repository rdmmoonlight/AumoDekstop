
using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Features.Shared.Http;
using AumoDekstop.Services;

namespace AumoDekstop.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private readonly ITokenStore _tokenStore;

    public UserProfileData? CachedProfile { get; private set; }
    public bool IsAuthenticated => _tokenStore.HasToken;

    public AuthService(ApiService? apiService = null, ITokenStore? tokenStore = null)
    {
        _apiService = apiService ?? ApiService.Instance;
        _tokenStore = tokenStore ?? TokenStore.Instance;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            ApplyToken(authData.Token);
            CachedProfile = null; // Invalidate cache profil
        }

        return ToAuthResponse(response);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<GoogleLoginRequest, AuthResponse>("api/v1/auth/google-login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            ApplyToken(authData.Token);
            CachedProfile = null;
        }

        return ToAuthResponse(response);
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
            var response = await _apiService.PostAsync<object, CommonActionResponse>("api/v1/auth/logout", new { }, ct);
            return response.Data ?? new CommonActionResponse(response.IsSuccess, response.ErrorMessage ?? string.Empty);
        }
        finally
        {
            ClearSession();
        }
    }

    public void ClearSession()
    {
        CachedProfile = null;
        _tokenStore.Clear();
        _apiService.SetAuthToken(null);
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
            0 or >= 500 => "Server belum siap atau tidak merespons. Silakan coba lagi.",
            _ => "Permintaan ditolak oleh server."
        };

        return new AuthResponse(false, message);
    }

    private void ApplyToken(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _tokenStore.Set(token);
            _apiService.SetAuthToken(token);
        }
    }
}