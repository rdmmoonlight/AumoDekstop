namespace AumoDekstop.Features.Shared.Http;

using System;

public interface ITokenStore
{
    string? Token { get; }
    bool HasToken { get; }
    void Set(string? token);
    void Clear();
    event EventHandler? SessionExpired;
    void RaiseSessionExpired();
}