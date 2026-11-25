
using System;

namespace AumoDekstop.Features.Shared.Http;
public interface ITokenStore
{
    string? Token { get; }
    bool HasToken { get; }
    void Set(string? token);
    void Clear();
    event EventHandler? SessionExpired;
    void RaiseSessionExpired();
}