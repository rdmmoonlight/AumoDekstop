using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Features.Shared.Http;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;

    public AuthHeaderHandler() : this(TokenStore.Instance)
    {
    }

    public AuthHeaderHandler(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // 1. Sebelum kirim: isi Bearer token jika tersedia
        if (request.Headers.Authorization is null && _tokenStore.HasToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        }

        // 2. Kirim permintaan sekali (tanpa retry di handler)
        var response = await base.SendAsync(request, cancellationToken);

        // 3. Sesudah terima: tangani 401 kecuali untuk endpoint login
        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsAuthEndpoint(request.RequestUri))
        {
            _tokenStore.RaiseSessionExpired();
        }

        return response;
    }

    private static bool IsAuthEndpoint(Uri? requestUri)
    {
        if (requestUri is null)
        {
            return false;
        }

        var path = requestUri.IsAbsoluteUri
            ? requestUri.AbsolutePath
            : requestUri.OriginalString;

        return path.EndsWith("auth/login", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith("auth/google-login", StringComparison.OrdinalIgnoreCase);
    }
}