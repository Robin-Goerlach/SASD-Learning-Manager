using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Protects complete tracker backup ZIP files with password-based authenticated encryption.
/// The envelope deliberately uses only .NET BCL cryptography so backup portability does not
/// depend on an additional package or a machine-specific Windows secret store.
/// </summary>
public sealed class PasswordProtectedBackupService
{
    private const int SaltLength = 16;
    private const int IvLength = 16;
    private const int EncryptionKeyLength = 32;
    private const int MacKeyLength = 32;
    private const int TagLength = 32;
    private const int IterationCount = 600_000;
    private const int MinimumPasswordLength = 12;
    private const int BufferSize = 128 * 1024;
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("SASDBK01");
    private static readonly int HeaderLength = Magic.Length + sizeof(int) + SaltLength + IvLength;

    /// <summary>Gets the minimum password length accepted for newly encrypted backups.</summary>
    public static int RequiredPasswordLength => MinimumPasswordLength;

    /// <summary>Returns whether the supplied file uses the SASD password-protected backup envelope.</summary>
    public bool IsEncryptedBackup(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return false;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < HeaderLength + TagLength)
        {
            return false;
        }

        Span<byte> magic = stackalloc byte[8];
        var read = stream.Read(magic);
        return read == Magic.Length && magic.SequenceEqual(Magic);
    }

    /// <summary>
    /// Encrypts a ZIP backup into the SASD envelope. AES-256-CBC provides streaming encryption while
    /// HMAC-SHA-256 authenticates the complete header and ciphertext before any decrypted file is used.
    /// </summary>
    public async Task EncryptFileAsync(
        string sourcePath,
        string targetPath,
        string password,
        CancellationToken cancellationToken = default)
    {
        ValidateNewPassword(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var fullSourcePath = Path.GetFullPath(sourcePath);
        var fullTargetPath = Path.GetFullPath(targetPath);
        if (!File.Exists(fullSourcePath))
        {
            throw new FileNotFoundException("Die zu verschlüsselnde Sicherung wurde nicht gefunden.", fullSourcePath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);
        var temporaryTarget = fullTargetPath + ".tmp";
        if (File.Exists(temporaryTarget))
        {
            File.Delete(temporaryTarget);
        }

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var iv = RandomNumberGenerator.GetBytes(IvLength);
        var keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            EncryptionKeyLength + MacKeyLength);

        try
        {
            var encryptionKey = keyMaterial.AsSpan(0, EncryptionKeyLength).ToArray();
            var macKey = keyMaterial.AsSpan(EncryptionKeyLength, MacKeyLength).ToArray();
            try
            {
                var header = BuildHeader(salt, iv);
                await using (var destination = new FileStream(
                                 temporaryTarget,
                                 FileMode.CreateNew,
                                 FileAccess.ReadWrite,
                                 FileShare.None,
                                 BufferSize,
                                 FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await destination.WriteAsync(header, cancellationToken).ConfigureAwait(false);

                    using var aes = Aes.Create();
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = encryptionKey;
                    aes.IV = iv;

                    await using (var source = new FileStream(
                                     fullSourcePath,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.Read,
                                     BufferSize,
                                     FileOptions.Asynchronous | FileOptions.SequentialScan))
                    await using (var crypto = new CryptoStream(destination, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
                    {
                        await source.CopyToAsync(crypto, BufferSize, cancellationToken).ConfigureAwait(false);
                        crypto.FlushFinalBlock();
                    }

                    await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
                    var authenticatedLength = destination.Position;
                    destination.Position = 0;
                    var tag = await ComputeHmacAsync(destination, authenticatedLength, macKey, cancellationToken).ConfigureAwait(false);
                    destination.Position = authenticatedLength;
                    await destination.WriteAsync(tag, cancellationToken).ConfigureAwait(false);
                    await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                File.Move(temporaryTarget, fullTargetPath, overwrite: true);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
                CryptographicOperations.ZeroMemory(macKey);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyMaterial);
            if (File.Exists(temporaryTarget))
            {
                File.Delete(temporaryTarget);
            }
        }
    }

    /// <summary>
    /// Decrypts a protected backup only after authenticating the full envelope. A wrong password and
    /// a modified file intentionally produce the same error message so callers cannot distinguish them.
    /// </summary>
    public async Task DecryptFileAsync(
        string sourcePath,
        string targetPath,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidDataException("Für diese verschlüsselte Sicherung ist ein Passwort erforderlich.");
        }

        var fullSourcePath = Path.GetFullPath(sourcePath);
        var fullTargetPath = Path.GetFullPath(targetPath);
        if (!File.Exists(fullSourcePath))
        {
            throw new FileNotFoundException("Die verschlüsselte Sicherung wurde nicht gefunden.", fullSourcePath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);
        var temporaryTarget = fullTargetPath + ".tmp";
        if (File.Exists(temporaryTarget))
        {
            File.Delete(temporaryTarget);
        }

        await using var source = new FileStream(
            fullSourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (source.Length < HeaderLength + TagLength + 16)
        {
            throw new InvalidDataException("Die verschlüsselte Sicherung ist unvollständig.");
        }

        var header = new byte[HeaderLength];
        await source.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        ParseHeader(header, out var iterations, out var salt, out var iv);

        var keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            EncryptionKeyLength + MacKeyLength);

        try
        {
            var encryptionKey = keyMaterial.AsSpan(0, EncryptionKeyLength).ToArray();
            var macKey = keyMaterial.AsSpan(EncryptionKeyLength, MacKeyLength).ToArray();
            try
            {
                var authenticatedLength = source.Length - TagLength;
                source.Position = authenticatedLength;
                var storedTag = new byte[TagLength];
                await source.ReadExactlyAsync(storedTag, cancellationToken).ConfigureAwait(false);

                source.Position = 0;
                var calculatedTag = await ComputeHmacAsync(source, authenticatedLength, macKey, cancellationToken).ConfigureAwait(false);
                if (!CryptographicOperations.FixedTimeEquals(storedTag, calculatedTag))
                {
                    throw new InvalidDataException("Passwort ist falsch oder die Sicherung wurde verändert.");
                }

                var cipherLength = authenticatedLength - HeaderLength;
                source.Position = HeaderLength;

                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encryptionKey;
                aes.IV = iv;

                try
                {
                    await using var limitedSource = new LimitedReadStream(source, cipherLength, leaveOpen: true);
                    await using var crypto = new CryptoStream(limitedSource, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);
                    await using var destination = new FileStream(
                        temporaryTarget,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        BufferSize,
                        FileOptions.Asynchronous | FileOptions.SequentialScan);
                    await crypto.CopyToAsync(destination, BufferSize, cancellationToken).ConfigureAwait(false);
                    await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (CryptographicException exception)
                {
                    throw new InvalidDataException("Passwort ist falsch oder die Sicherung wurde verändert.", exception);
                }

                File.Move(temporaryTarget, fullTargetPath, overwrite: true);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
                CryptographicOperations.ZeroMemory(macKey);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyMaterial);
            if (File.Exists(temporaryTarget))
            {
                File.Delete(temporaryTarget);
            }
        }
    }

    /// <summary>Validates a password selected for a newly encrypted backup.</summary>
    public static void ValidateNewPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException(
                $"Das Backup-Passwort muss mindestens {MinimumPasswordLength} Zeichen lang sein.",
                nameof(password));
        }
    }

    private static byte[] BuildHeader(byte[] salt, byte[] iv)
    {
        var header = new byte[HeaderLength];
        Magic.CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(Magic.Length, sizeof(int)), IterationCount);
        salt.CopyTo(header, Magic.Length + sizeof(int));
        iv.CopyTo(header, Magic.Length + sizeof(int) + SaltLength);
        return header;
    }

    private static void ParseHeader(byte[] header, out int iterations, out byte[] salt, out byte[] iv)
    {
        if (header.Length != HeaderLength || !header.AsSpan(0, Magic.Length).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Die Datei ist kein unterstütztes verschlüsseltes SASD-Backup.");
        }

        iterations = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(Magic.Length, sizeof(int)));
        if (iterations < 100_000 || iterations > 5_000_000)
        {
            throw new InvalidDataException("Die KDF-Parameter der Sicherung sind ungültig.");
        }

        salt = header.AsSpan(Magic.Length + sizeof(int), SaltLength).ToArray();
        iv = header.AsSpan(Magic.Length + sizeof(int) + SaltLength, IvLength).ToArray();
    }

    private static async Task<byte[]> ComputeHmacAsync(
        Stream stream,
        long byteCount,
        byte[] macKey,
        CancellationToken cancellationToken)
    {
        using var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);
        var buffer = new byte[BufferSize];
        var remaining = byteCount;
        while (remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requested = (int)Math.Min(buffer.Length, remaining);
            var read = await stream.ReadAsync(buffer.AsMemory(0, requested), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new EndOfStreamException("Die verschlüsselte Sicherung endete unerwartet.");
            }

            hmac.AppendData(buffer.AsSpan(0, read));
            remaining -= read;
        }

        return hmac.GetHashAndReset();
    }

    /// <summary>
    /// Read-only stream window used to ensure the trailing HMAC is never fed into the AES decryptor.
    /// </summary>
    private sealed class LimitedReadStream(Stream inner, long length, bool leaveOpen) : Stream
    {
        private long _remaining = length;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position
        {
            get => length - _remaining;
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining <= 0)
            {
                return 0;
            }

            var read = inner.Read(buffer, offset, (int)Math.Min(count, _remaining));
            _remaining -= read;
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            if (_remaining <= 0)
            {
                return 0;
            }

            var read = inner.Read(buffer[..(int)Math.Min(buffer.Length, _remaining)]);
            _remaining -= read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_remaining <= 0)
            {
                return 0;
            }

            var requested = (int)Math.Min(buffer.Length, _remaining);
            var read = await inner.ReadAsync(buffer[..requested], cancellationToken).ConfigureAwait(false);
            _remaining -= read;
            return read;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !leaveOpen)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            if (!leaveOpen)
            {
                return inner.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
