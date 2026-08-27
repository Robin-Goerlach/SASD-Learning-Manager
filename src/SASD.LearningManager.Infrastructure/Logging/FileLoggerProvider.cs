using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SASD.LearningManager.Infrastructure.Logging;

/// <summary>
/// Minimal local rolling-by-day file logger for the desktop application. It intentionally avoids
/// logging domain payloads and exists so first-line diagnostics do not depend on a cloud service.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.Ordinal);
    private readonly string _logDirectory;
    private bool _disposed;

    public FileLoggerProvider(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(logDirectory);
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FileLoggerProvider));
        return _loggers.GetOrAdd(categoryName, category => new FileLogger(category, _logDirectory));
    }

    public void Dispose()
    {
        _disposed = true;
        _loggers.Clear();
    }

    private sealed class FileLogger : ILogger
    {
        private static readonly object FileLock = new();
        private readonly string _category;
        private readonly string _logDirectory;

        public FileLogger(string category, string logDirectory)
        {
            _category = category;
            _logDirectory = logDirectory;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var now = DateTimeOffset.Now;
            var file = Path.Combine(_logDirectory, $"learning-manager-{now:yyyy-MM-dd}.log");
            var message = formatter(state, exception);
            var line = $"{now:O} [{logLevel}] {_category} ({eventId.Id}): {message}";
            if (exception is not null)
            {
                line += Environment.NewLine + exception;
            }

            lock (FileLock)
            {
                try
                {
                    File.AppendAllText(file, line + Environment.NewLine, System.Text.Encoding.UTF8);
                }
                catch (IOException)
                {
                    // Logging is diagnostic support. A temporarily unavailable log file must not
                    // turn an otherwise recoverable application operation into a crash.
                }
                catch (UnauthorizedAccessException)
                {
                    // The same best-effort rule applies when the log directory becomes read-only.
                }
            }
        }
    }
}
