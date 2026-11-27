using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AumoDekstop.Services.Api;

internal static class ApiRetryPolicy
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(600);

    public static int MaxAttempts(bool isIdempotent) => isIdempotent ? MaxRetries + 1 : 1;

    public static TimeSpan GetDelay(int attempt) =>
        TimeSpan.FromMilliseconds(BaseRetryDelay.TotalMilliseconds * attempt);

    public static bool IsTransientStatus(HttpStatusCode code) =>
        code is HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    public static bool IsTransientException(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested);
}