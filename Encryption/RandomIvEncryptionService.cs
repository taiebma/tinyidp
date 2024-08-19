using System.Security.Cryptography;
using tinyidp.Exceptions;

namespace tinyidp.Encryption;

public class RandomIvEncryptionService : IEncryptionService
{
    private readonly IConfiguration _conf;
    private byte[] encryptionKey;

    public RandomIvEncryptionService(IConfiguration conf)
    {
        _conf = conf;

        string? key = _conf.GetSection("TINYIDP_SECU")?.GetValue<string>("Key");
        if (key == null)
        {
            key = GenerateKey();
            SaveKey(key);
        }
        encryptionKey = System.Convert.FromBase64String(key);
    }

    public string Encrypt(string plaintext)
    {
        byte[] cyphertextBytes;
        using var aes = Aes.Create();
        var encryptor = aes.CreateEncryptor(encryptionKey, aes.IV);
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plaintext);
                }
            }
            cyphertextBytes = memoryStream.ToArray();
            
            return new AesCbcCiphertext(aes.IV, cyphertextBytes).ToString();
        }
    }

    public string Decrypt(string ciphertext)
    {
        AesCbcCiphertext cbcCiphertext;
        try
        {
            cbcCiphertext = AesCbcCiphertext.FromBase64String(ciphertext);
        }
        catch (FormatException ex)
        {
            throw new TinyidpKeyException("Invalid key format", ex);
        }
        using var aes = Aes.Create();
        var decryptor = aes.CreateDecryptor(encryptionKey, cbcCiphertext.Iv);
        try
        {
            using (var memoryStream = new MemoryStream(cbcCiphertext.CiphertextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
        catch (CryptographicException ex)
        {
            throw new TinyidpKeyException("Invalid key", ex);
        }
    }

    public string GenerateKey()
    {
        using (Aes aesAlgorithm = Aes.Create())
        {
            aesAlgorithm.KeySize = 256;
            aesAlgorithm.GenerateKey();
            string keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
            return keyBase64;
        }        
    }

    private void SaveKey(string key)
    {
        StreamWriter writer = new StreamWriter(String.Format("{0}/tinyidp.key", Environment.GetEnvironmentVariable("TINYIDP_SECU__PATH")));
        writer.WriteLine("{ \"TINYIDP_SECU:Key\": \"" + key + "\" }");
        writer.Close();
    }
}
