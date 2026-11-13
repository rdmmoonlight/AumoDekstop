using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AumoDekstop.Services;

public sealed class UpdateInfo
{
    public string VersionTag { get; init; } = string.Empty;
    public string ReleaseNotes { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}

public class GitHubUpdateService
{
    // Ganti dengan username & nama repo GitHub Anda
    private const string RepoOwner = "rdmmoonlight";
    private const string RepoName = "AumoDekstop";

    private static readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            // GitHub API wajib menyertakan User-Agent
            { "User-Agent", "AumoDekstop-Updater" }
        }
    };

    /// <summary>
    /// Mengambil versi aplikasi saat ini dari InformationalVersion (contoh: 26.11.1.PC)
    /// </summary>
    public static string GetCurrentVersionString()
    {
        var infoVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0.PC";

        // Bersihkan metadata commit hash (+abc1234) jika ada
        int plusIndex = infoVersion.IndexOf('+');
        return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
    }

    /// <summary>
    /// Mengecek apakah ada versi .PC yang lebih baru di GitHub Releases.
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases?per_page=15";
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var releases = JsonSerializer.Deserialize<GitHubRelease[]>(json);

            if (releases == null || releases.Length == 0)
                return null;

            // Cari rilis pertama (terbaru) yang bukan draft/prerelease dan berakhiran .PC
            var latestPcRelease = releases.FirstOrDefault(r =>
                !r.Draft &&
                !r.Prerelease &&
                r.TagName.EndsWith(".PC", StringComparison.OrdinalIgnoreCase));

            if (latestPcRelease == null)
                return null;

            Version currentVersion = ParseCalVer(GetCurrentVersionString());
            Version latestVersion = ParseCalVer(latestPcRelease.TagName);

            if (latestVersion > currentVersion)
            {
                // Cari file installer .exe di dalam aset rilis
                var installerAsset = latestPcRelease.Assets.FirstOrDefault(a =>
                    a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

                if (installerAsset != null)
                {
                    return new UpdateInfo
                    {
                        VersionTag = latestPcRelease.TagName,
                        ReleaseNotes = latestPcRelease.Body ?? "Pembaruan versi terbaru tersedia.",
                        DownloadUrl = installerAsset.BrowserDownloadUrl,
                        FileName = installerAsset.Name
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AutoUpdate] Gagal mengecek update: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Mengunduh installer ke folder Temp dan menjalankannya secara otomatis.
    /// </summary>
    public async Task DownloadAndInstallAsync(UpdateInfo update, IProgress<double>? progress = null)
    {
        string tempFilePath = Path.Combine(Path.GetTempPath(), update.FileName);

        using var response = await _httpClient.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                double percentage = (double)totalRead / totalBytes.Value * 100;
                progress?.Report(percentage);
            }
        }

        fileStream.Close();

        // Jalankan Inno Setup secara silent dan otomatis buka ulang aplikasi setelah selesai
        Process.Start(new ProcessStartInfo
        {
            FileName = tempFilePath,
            Arguments = "/SILENT /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS",
            UseShellExecute = true
        });

        // Tutup aplikasi saat ini agar file .exe bisa ditimpa installer
        Environment.Exit(0);
    }

    private static Version ParseCalVer(string tag)
    {
        // Ubah "v26.11.2.PC" -> "26.11.2" agar bisa dibandingkan dengan class Version C#
        string clean = tag.TrimStart('v', 'V');
        if (clean.EndsWith(".PC", StringComparison.OrdinalIgnoreCase))
            clean = clean[..^3];

        return Version.TryParse(clean, out var parsed) ? parsed : new Version(0, 0, 0);
    }

    // Model JSON GitHub API
    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("assets")]
        public GitHubAsset[] Assets { get; set; } = [];
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}
