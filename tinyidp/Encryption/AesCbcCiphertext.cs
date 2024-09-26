namespace tinyidp.Encryption;

public class AesCbcCiphertext
{
    public byte[] Iv { get; }
    public byte[] CiphertextBytes { get; }

    public static AesCbcCiphertext FromBase64String(string data)
    {
        var dataBytes = Convert.FromBase64String(data);
        return new AesCbcCiphertext(
            dataBytes.Take(16).ToArray(),
            dataBytes.Skip(16).ToArray()
        );
    }

    public AesCbcCiphertext(byte[] iv, byte[] ciphertextBytes)
    {
        Iv = iv;
        CiphertextBytes = ciphertextBytes;
    }

    public override string ToString()
    {
        return Convert.ToBase64String(Iv.Concat(CiphertextBytes).ToArray());
    }
}