using Ayws.Security.Service.Application.Contracts.Services.Encryption;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Ayws.Security.Service.Infrastructure.Services;

public class EncryptionService(IOptions<EncryptionSettings> options) : IEncryptionService
{
    private readonly byte[] _key = Convert.FromBase64String(options.Value.SecretEncryptionKey);

    public string Encrypt(string plainText)
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: nonce (12) + tag (16) + cipher
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        cipherBytes.CopyTo(result, nonce.Length + tag.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        var nonce = data[..12];
        var tag = data[12..28];
        var cipher = data[28..];
        var plainBytes = new byte[cipher.Length];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}

public class EncryptionSettings
{
    public const string Key = "EncryptionSettings";
    /// <summary>Base64 encoded 256-bit key</summary>
    public string SecretEncryptionKey { get; set; } = null!;
}
