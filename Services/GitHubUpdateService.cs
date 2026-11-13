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
    private const string RepoOwner = "rdmmoonlight";
    private const string RepoName = "AumoDekstop";

    private static readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            // GitHub API wajib menyertakan User-Agent
            { "User-Agent", "AumoDekstop-Portable-Updater" }
        }
    };

    /// <summary>
    /// Membersihkan sisa file executable lama (.old) dari proses update sebelumnya.
    /// Panggil method ini di constructor MainWindow().
    /// </summary>
    public static void CleanupOldExecutable()
    {
        try
        {
            string? currentExe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(currentExe))
            {
                string oldExe = currentExe + ".old";
                if (File.Exists(oldExe))
                {
                    File.Delete(oldExe);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AutoUpdate] Gagal menghapus file .old: {ex.Message}");
        }
    }

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
    /// Mengecek apakah ada versi .PC portabel yang lebih baru di GitHub Releases.
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
                // Cari file executable portabel (.exe) dan pastikan bukan file Setup installer
                var portableAsset = latestPcRelease.Assets.FirstOrDefault(a =>
                    a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                    !a.Name.StartsWith("Setup", StringComparison.OrdinalIgnoreCase));

                if (portableAsset != null)
                {
                    return new UpdateInfo
                    {
                        VersionTag = latestPcRelease.TagName,
                        ReleaseNotes = latestPcRelease.Body ?? "Pembaruan versi terbaru tersedia.",
                        DownloadUrl = portableAsset.BrowserDownloadUrl,
                        FileName = portableAsset.Name
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
    /// Mengunduh executable baru ke direktori aplikasi saat ini, menukar file aktif (.old),
    /// lalu menjalankan ulang versi terbaru secara otomatis (mode portabel).
    /// </summary>
    public async Task DownloadAndInstallAsync(UpdateInfo update, IProgress<double>? progress = null)
    {
        string currentExe = Environment.ProcessPath
            ?? throw new InvalidOperationException("Lokasi executable saat ini tidak dapat dideteksi.");

        string newExe = currentExe + ".new";
        string oldExe = currentExe + ".old";

        try
        {
            // 1. Unduh file .exe baru langsung ke folder aplikasi dengan ekstensi .new
            // (Harus di drive/folder yang sama agar File.Move berjalan instan tanpa cross-volume copy)
            using var response = await _httpClient.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using (var fileStream = new FileStream(newExe, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
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
            }

            // 2. Hapus sisa file .old jika masih tertinggal
            if (File.Exists(oldExe))
            {
                File.Delete(oldExe);
            }

            // 3. Rename .exe yang sedang berjalan -> .old, lalu .new -> .exe asli
            File.Move(currentExe, oldExe);
            File.Move(newExe, currentExe);

            // 4. Jalankan executable versi baru dan tutup proses lama
            Process.Start(new ProcessStartInfo
            {
                FileName = currentExe,
                WorkingDirectory = Path.GetDirectoryName(currentExe),
                UseShellExecute = true
            });

            Environment.Exit(0);
        }
        catch
        {
            // Bersihkan file .new yang gagal/korup agar tidak mengotori folder user
            if (File.Exists(newExe))
            {
                try { File.Delete(newExe); } catch { }
            }
            throw;
        }
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
