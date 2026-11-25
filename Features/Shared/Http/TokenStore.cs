
using System;

namespace AumoDekstop.Features.Shared.Http;
public sealed class TokenStore : ITokenStore
{
    private static readonly Lazy<TokenStore> _instance = new(() => new TokenStore());
    public static TokenStore Instance => _instance.Value;

    private readonly object _lock = new();
    private string? _token;

    public TokenStore()
    {
    }

    public string? Token
    {
        get
        {
            lock (_lock)
            {
                return _token;
            }
        }
    }

    public bool HasToken
    {
        get
        {
            lock (_lock)
            {
                return !string.IsNullOrWhiteSpace(_token);
            }
        }
    }

    public event EventHandler? SessionExpired;

    public void Set(string? token)
    {
        lock (_lock)
        {
            _token = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _token = null;
        }
    }

    public void RaiseSessionExpired()
    {
        EventHandler? handlerToInvoke = null;

        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                return;
            }

            _token = null;
            handlerToInvoke = SessionExpired;
        }

        // Event dipicu di luar lock untuk mencegah deadlock jika subscriber mengakses TokenStore
        handlerToInvoke?.Invoke(this, EventArgs.Empty);
    }
}