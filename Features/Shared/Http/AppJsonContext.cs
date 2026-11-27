using System.Text.Json.Serialization;
using AumoDekstop.Features.Auth.Models;

namespace AumoDekstop.Features.Shared.Http;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(GoogleLoginRequest))]
[JsonSerializable(typeof(AuthResponse))]
[JsonSerializable(typeof(ProfileResponse))]
[JsonSerializable(typeof(UserProfileData))]
[JsonSerializable(typeof(CommonActionResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(SimpleAuthResponse))]
[JsonSerializable(typeof(LoginPayload))]
[JsonSerializable(typeof(RegisterPayload))]
public partial class AppJsonContext : JsonSerializerContext
{
}