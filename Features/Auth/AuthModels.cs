using System.Text.Json.Serialization;

namespace AumoDekstop.Features.Auth.Models;

// ==========================================
// 1. Model dari types.ts
// ==========================================
public record User(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl = null
);

public record SimpleAuthResponse(
    [property: JsonPropertyName("user")] User User,
    [property: JsonPropertyName("token")] string Token
);

public record LoginPayload(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("code")] string? Code = null
);

public record RegisterPayload(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string? Name = null
);

// ==========================================
// 2. Model dari auth.api.ts
// ==========================================
public record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);

public record GoogleLoginRequest(
    [property: JsonPropertyName("idToken")] string IdToken
);

public record AuthResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("userId")] string? UserId = null,
    [property: JsonPropertyName("fullName")] string? FullName = null,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl = null,
    [property: JsonPropertyName("token")] string? Token = null,
    [property: JsonPropertyName("lockoutEnd")] DateTimeOffset? LockoutEnd = null,
    [property: JsonPropertyName("errors")] List<string>? Errors = null
);

public record UserProfileData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("phoneNumber")] string? PhoneNumber = null,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl = null,
    [property: JsonPropertyName("bio")] string? Bio = null,
    [property: JsonPropertyName("roles")] List<string>? Roles = null,
    [property: JsonPropertyName("customClaims")] List<Dictionary<string, object>>? CustomClaims = null
);

public record ProfileResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] UserProfileData? Data,
    [property: JsonPropertyName("message")] string? Message = null
);

public record CommonActionResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message
);