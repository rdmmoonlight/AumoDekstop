
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Features.Shared.Http;

public sealed class ColdStartRetryHandler : DelegatingHandler
{
    private const int MaxRetries = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Baca payload satu kali sebelum percobaan pertama agar bisa dikloning ulang
        byte[]? contentBytes = request.Content != null
            ? await request.Content.ReadAsByteArrayAsync(cancellationToken)
            : null;

        bool isGetOrPut = request.Method == HttpMethod.Get || request.Method == HttpMethod.Put;

        for (int attempt = 0; ; attempt++)
        {
            using var clonedRequest = CloneRequest(request, contentBytes);

            try
            {
                var response = await base.SendAsync(clonedRequest, cancellationToken);

                if (attempt < MaxRetries && ShouldRetryStatus(response.StatusCode, isGetOrPut))
                {
                    // Bebaskan respons gagal sebelum mengulang
                    response.Dispose();
                }
                else
                {
                    return response;
                }
            }
            catch (HttpRequestException) when (isGetOrPut && attempt < MaxRetries)
            {
                // Lanjut ke jeda retry untuk GET dan PUT
            }

            await Task.Delay(RetryDelay, cancellationToken);
        }
    }

    private static bool ShouldRetryStatus(HttpStatusCode statusCode, bool isGetOrPut) =>
        statusCode == HttpStatusCode.BadGateway ||
        statusCode == HttpStatusCode.ServiceUnavailable ||
        (isGetOrPut && statusCode == HttpStatusCode.GatewayTimeout);

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? contentBytes)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
            VersionPolicy = original.VersionPolicy
        };

        // Salin request headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Salin HttpRequestOptions (.NET 5+)
        foreach (var option in original.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        // Salin isi dan content headers jika ada
        if (original.Content != null && contentBytes != null)
        {
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}