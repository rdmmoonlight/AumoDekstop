using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Windows.System;

namespace AumoDekstop.Features.Auth.Services;

public sealed class DesktopGoogleAuthProvider : IGoogleAuthProvider
{
    private static readonly HttpClient OAuthHttpClient = new();

    public async Task<string?> SignInAndGetIdTokenAsync(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("GOOGLE_CLIENT_ID belum dikonfigurasi.");

        // 1. Cari port localhost kosong untuk loopback redirect URI
        int port = GetRandomUnusedPort();
        string redirectUri = $"http://127.0.0.1:{port}/";

        // 2. Buat PKCE code_verifier & code_challenge (standar keamanan OAuth Desktop)
        string codeVerifier = GenerateRandomBase64Url(32);
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string state = GenerateRandomBase64Url(16);

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        try
        {
            string authUrl =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                "&response_type=code" +
                "&scope=openid%20email%20profile" +
                $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                "&code_challenge_method=S256" +
                $"&state={Uri.EscapeDataString(state)}";

            // 3. Buka browser default Windows
            await Launcher.LaunchUriAsync(new Uri(authUrl));

            // 4. Tunggu callback dari Google (maksimal 2 menit)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(2));

            var contextTask = listener.GetContextAsync();
            var completed = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, timeoutCts.Token));
            if (completed != contextTask)
                return null;

            var context = await contextTask;
            var query = context.Request.QueryString;
            string? code = query["code"];
            string? returnedState = query["state"];
            string? error = query["error"];

            // Kirim halaman konfirmasi singkat ke browser user
            string html = string.IsNullOrEmpty(error)
                ? "<html><body style='font-family:sans-serif;text-align:center;padding-top:40px;'><h2>Login Berhasil</h2><p>Silakan kembali ke aplikasi Aumo Desktop.</p><script>window.close()</script></body></html>"
                : "<html><body style='font-family:sans-serif;text-align:center;padding-top:40px;'><h2>Login Dibatalkan</h2><p>Anda dapat menutup tab ini.</p></body></html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, cancellationToken);
            context.Response.OutputStream.Close();

            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code) || returnedState != state)
                return null;

            // 5. Tukar authorization code dengan id_token dari Google Token Endpoint
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri
            });

            using var tokenRes = await OAuthHttpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest, cancellationToken);
            string tokenJson = await tokenRes.Content.ReadAsStringAsync(cancellationToken);

            if (!tokenRes.IsSuccessStatusCode)
                throw new InvalidOperationException("Gagal menukar kode otorisasi Google.");

            using var doc = JsonDocument.Parse(tokenJson);
            return doc.RootElement.TryGetProperty("id_token", out var idTokenProp)
                ? idTokenProp.GetString()
                : null;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static int GetRandomUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string GenerateRandomBase64Url(int byteLength)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}