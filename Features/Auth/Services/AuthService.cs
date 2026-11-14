using AumoDekstop.Features.Auth.Models;
using AumoDekstop.Services;

namespace AumoDekstop.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private string? _accessToken;

    public UserProfileData? CachedProfile { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken);

    // Default menggunakan ApiService.Instance agar konsisten di seluruh window/page
    public AuthService(ApiService? apiService = null)
    {
        _apiService = apiService ?? ApiService.Instance;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            ApplyToken(authData.Token);
            CachedProfile = null; // Invalidate cache profil
        }

        return response.Data!;
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        var response = await _apiService.PostAsync<GoogleLoginRequest, AuthResponse>("api/v1/auth/google-login", request, ct);

        if (response.IsSuccess && response.Data is { Success: true } authData)
        {
            ApplyToken(authData.Token);
            CachedProfile = null;
        }

        return response.Data!;
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

        return response.Data!;
    }

    public async Task<CommonActionResponse> LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            // Endpoint logout tanpa body payload
            var response = await _apiService.PostAsync<object, CommonActionResponse>("api/v1/auth/logout", new { }, ct);
            return response.Data!;
        }
        finally
        {
            ClearSession();
        }
    }

    public void ClearSession()
    {
        _accessToken = null;
        CachedProfile = null;
        _apiService.SetAuthToken(null);
    }

    private void ApplyToken(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _accessToken = token;
            _apiService.SetAuthToken(token);
        }
    }
}