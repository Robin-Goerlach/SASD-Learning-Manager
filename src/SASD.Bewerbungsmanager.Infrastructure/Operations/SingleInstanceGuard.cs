namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>Owns the named mutex that protects the single-user SQLite desktop session.</summary>
public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;
    private bool _ownsMutex;

    private SingleInstanceGuard(Mutex mutex, bool ownsMutex)
    {
        _mutex = mutex;
        _ownsMutex = ownsMutex;
    }

    /// <summary>
    /// Attempts to acquire the process/session guard. Returns <see langword="null"/> when another
    /// interactive instance already owns it.
    /// </summary>
    public static SingleInstanceGuard? TryAcquire()
    {
        var mutex = new Mutex(initiallyOwned: true, @"Local\SASD.Bewerbungsmanager.v1", out var createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            return null;
        }

        return new SingleInstanceGuard(mutex, ownsMutex: true);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsMutex)
        {
            _mutex.ReleaseMutex();
            _ownsMutex = false;
        }

        _mutex.Dispose();
    }
}
