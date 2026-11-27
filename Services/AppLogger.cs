using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AumoDekstop.Services;

public static class AppLogger
{
    private const string FileName = "crash_log.txt";
    private const long MaxBytes = 200_000;
    public const string EmptyMessage = "Belum ada log.";

    private static readonly object FileLock = new();
    private static readonly Channel<LogEntry> Queue = Channel.CreateUnbounded<LogEntry>(
        new UnboundedChannelOptions { SingleReader = true });

    private static string? _logFilePath;
    private static int _initialized;

    private readonly record struct LogEntry(
        DateTime Timestamp,
        string Level,
        string Tag,
        string Message,
        Exception? Exception);

    public static string LogFilePath => _logFilePath ?? string.Empty;

    /// <summary>
    /// Panggil sekali di App.xaml.cs saat aplikasi mulai.
    /// Aman untuk unpackaged maupun MSIX packaged WinUI 3.
    /// </summary>
    public static void Init(string? customDirectory = null)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        var baseDir = customDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AumoDekstop",
                "Logs");

        Directory.CreateDirectory(baseDir);
        _logFilePath = Path.Combine(baseDir, FileName);

        _ = Task.Factory.StartNew(
            ProcessQueueAsync,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public static void Info(string tag, string message)
    {
        Debug.WriteLine($"[INFO/{tag}] {message}");
        Enqueue("INFO", tag, message, null);
    }

    public static void Warn(string tag, string message)
    {
        Debug.WriteLine($"[WARN/{tag}] {message}");
        Enqueue("WARN", tag, message, null);
    }

    public static void Error(string tag, string message, Exception? exception = null)
    {
        // Pembatalan Task/CancellationToken bukan error.
        if (exception is OperationCanceledException)
            return;

        Debug.WriteLine($"[ERROR/{tag}] {message}\n{exception}");
        Enqueue("ERROR", tag, message, exception);
    }

    /// <summary>
    /// Khusus crash tak tertangani. Ditulis SINKRON di thread pemanggil
    /// karena proses akan segera mati; penulisan lewat antrean bisa hilang.
    /// </summary>
    public static void Fatal(string tag, string message, Exception? exception = null)
    {
        Debug.WriteLine($"[FATAL/{tag}] {message}\n{exception}");
        WriteNow(DateTime.Now, "FATAL", tag, message, exception);
    }

    public static string Read()
    {
        try
        {
            lock (FileLock)
            {
                if (string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
                    return EmptyMessage;

                var info = new FileInfo(_logFilePath);
                if (info.Length == 0)
                    return EmptyMessage;

                return File.ReadAllText(_logFilePath, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            return $"Gagal membaca log: {ex.Message}";
        }
    }

    public static void Clear()
    {
        try
        {
            lock (FileLock)
            {
                if (!string.IsNullOrEmpty(_logFilePath) && File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, string.Empty, Encoding.UTF8);
                }
            }
        }
        catch
        {
            // Jangan biarkan logging menyebabkan crash tambahan.
        }
    }

    private static void Enqueue(string level, string tag, string message, Exception? exception)
    {
        if (string.IsNullOrEmpty(_logFilePath))
            Init();

        Queue.Writer.TryWrite(new LogEntry(DateTime.Now, level, tag, message, exception));
    }

    private static async Task ProcessQueueAsync()
    {
        await foreach (var entry in Queue.Reader.ReadAllAsync())
        {
            WriteNow(entry.Timestamp, entry.Level, entry.Tag, entry.Message, entry.Exception);
        }
    }

    private static void WriteNow(
        DateTime timestamp,
        string level,
        string tag,
        string message,
        Exception? exception)
    {
        try
        {
            lock (FileLock)
            {
                if (string.IsNullOrEmpty(_logFilePath))
                    return;

                var time = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                var trace = exception is not null ? $"\n{FormatException(exception)}" : string.Empty;

                // Batasi ukuran berkas: buang separuh bagian lama bila melewati batas.
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length > MaxBytes)
                    {
                        var existing = File.ReadAllText(_logFilePath, Encoding.UTF8);
                        var half = existing.Substring(existing.Length / 2);
                        File.WriteAllText(_logFilePath, half, Encoding.UTF8);
                    }
                }

                File.AppendAllText(
                    _logFilePath,
                    $"\n[{time}] {level}/{tag}: {message}{trace}\n",
                    Encoding.UTF8);
            }
        }
        catch
        {
            // Jangan biarkan logging menyebabkan crash tambahan.
        }
    }

    private static string FormatException(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        var depth = 0;

        while (current is not null)
        {
            if (depth > 0)
                sb.AppendLine($"--- Inner Exception ({depth}) ---");

            sb.AppendLine($"{current.GetType().FullName}: {current.Message}");
            if (!string.IsNullOrWhiteSpace(current.StackTrace))
                sb.AppendLine(current.StackTrace);

            current = current.InnerException;
            depth++;
        }

        return sb.ToString().TrimEnd();
    }
}