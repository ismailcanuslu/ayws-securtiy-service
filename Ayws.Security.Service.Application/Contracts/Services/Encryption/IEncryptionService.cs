namespace Ayws.Security.Service.Application.Contracts.Services.Encryption;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
