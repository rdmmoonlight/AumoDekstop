namespace AumoDekstop.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Penyimpanan pengaturan lokal berbasis file JSON di %LOCALAPPDATA%\AumoDekstop\settings.json.
/// Pengganti ApplicationData.Current.LocalSettings, yang melempar exception pada aplikasi
/// unpackaged (WindowsPackageType=None) karena tidak memiliki package identity.
/// </summary>
public static class AppLocalSettings
{
    private static readonly object _lock = new();
    private static readonly string _folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AumoDekstop");
    private static readonly string _filePath = Path.Combine(_folder, "settings.json");

    private static Dictionary<string, string>? _cache;

    public static string? GetString(string key)
    {
        lock (_lock)
        {
            var data = Load();
            return data.TryGetValue(key, out var value) ? value : null;
        }
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        var raw = GetString(key);
        return bool.TryParse(raw, out var parsed) ? parsed : defaultValue;
    }

    public static void SetString(string key, string value)
    {
        lock (_lock)
        {
            var data = Load();
            data[key] = value;
            Save(data);
        }
    }

    public static void SetBool(string key, bool value) => SetString(key, value.ToString());

    public static void Remove(string key)
    {
        lock (_lock)
        {
            var data = Load();
            if (data.Remove(key))
            {
                Save(data);
            }
        }
    }

    private static Dictionary<string, string> Load()
    {
        if (_cache is not null) return _cache;

        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                         ?? new Dictionary<string, string>();
                return _cache;
            }
        }
        catch
        {
            // File rusak atau tidak terbaca: mulai dari pengaturan kosong
        }

        _cache = new Dictionary<string, string>();
        return _cache;
    }

    private static void Save(Dictionary<string, string> data)
    {
        try
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(data));
        }
        catch
        {
            // Gagal menulis tidak boleh membuat aplikasi crash
        }
    }
}
