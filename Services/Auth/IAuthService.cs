using AumoDekstop.Models.Auth;

namespace AumoDekstop.Services.Auth;

public interface IAuthService
{
    UserProfileData? CachedProfile { get; }
    bool IsAuthenticated { get; }

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse> GetProfileAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<CommonActionResponse> LogoutAsync(CancellationToken cancellationToken = default);
    void ClearSession();
}