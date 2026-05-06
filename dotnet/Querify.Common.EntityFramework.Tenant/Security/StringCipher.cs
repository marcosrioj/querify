using System.Security.Cryptography;
using System.Text;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using System.Net;

namespace Querify.Common.EntityFramework.Tenant.Security;

public sealed class StringCipher
{
    public static StringCipher Instance { get; } = new();

    public const int DefaultKeySizeBits = 256;
    public const int DefaultSaltSizeBytes = 16;
    public const int DefaultIvSizeBytes = 16;
    public const int DefaultIterations = 210_000;

    public static string DefaultPassPhrase { get; set; } = "CHANGE_ME_2026";

    public string? Encrypt(string plainText, CipherOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(plainText)) return null;
        options ??= CipherOptions.Default;

        var salt = RandomNumberGenerator.GetBytes(options.SaltSizeBytes);
        var iv = RandomNumberGenerator.GetBytes(options.IvSizeBytes);

        var key = DeriveKey(
            options.PassPhrase ?? DefaultPassPhrase,
            salt,
            options.Iterations,
            options.KeySizeBits);

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(key, iv);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write, leaveOpen: true))
        {
            cs.Write(plainBytes);
            cs.FlushFinalBlock();
        }

        var cipherBytes = ms.ToArray();

        // payload = salt | iv | cipher
        var payload = new byte[salt.Length + iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
        Buffer.BlockCopy(iv, 0, payload, salt.Length, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, salt.Length + iv.Length, cipherBytes.Length);

        return "v1:" + Convert.ToBase64String(payload);
    }

    public string? Decrypt(string cipherText, CipherOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(cipherText)) return null;
        options ??= CipherOptions.Default;

        if (!cipherText.StartsWith("v1:", StringComparison.Ordinal))
            throw new ApiErrorException(
                "Unsupported cipher format.",
                errorCode: (int)HttpStatusCode.BadRequest);

        var payload = Convert.FromBase64String(cipherText["v1:".Length..]);

        var salt = payload[..options.SaltSizeBytes];
        var iv = payload.AsSpan(options.SaltSizeBytes, options.IvSizeBytes);
        var cipherBytes = payload[(options.SaltSizeBytes + options.IvSizeBytes)..];

        var key = DeriveKey(
            options.PassPhrase ?? DefaultPassPhrase,
            salt,
            options.Iterations,
            options.KeySizeBits);

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor(key, iv.ToArray());
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var plainMs = new MemoryStream();

        cs.CopyTo(plainMs);
        return Encoding.UTF8.GetString(plainMs.ToArray());
    }

    private static byte[] DeriveKey(string passPhrase, byte[] salt, int iterations, int keySizeBits)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            passPhrase,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            keySizeBits / 8);
    }
}

public sealed record CipherOptions
{
    public static CipherOptions Default { get; } = new();

    public string? PassPhrase { get; init; }
    public int KeySizeBits { get; init; } = StringCipher.DefaultKeySizeBits;
    public int SaltSizeBytes { get; init; } = StringCipher.DefaultSaltSizeBytes;
    public int IvSizeBytes { get; init; } = StringCipher.DefaultIvSizeBytes;
    public int Iterations { get; init; } = StringCipher.DefaultIterations;
}