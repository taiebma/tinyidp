
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace tinyidp.Encryption;

public class PKCEService : IPKCEService
{
    private readonly IConfiguration _conf;
    private readonly ILogger<PKCEService> _logger;

    public PKCEService(IConfiguration conf, ILogger<PKCEService> logger)
    {
        _conf = conf;
        _logger = logger;
    }

    public bool ValidPKCE(string code_verifier, string CodeChallenge)
    {
        string codeHash64;

        using (SHA256 mySHA256 = SHA256.Create())
        {
            byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(code_verifier));
            codeHash64 = Base64UrlEncode(hashValue);
        }
        return codeHash64.Equals(CodeChallenge);
    }

    public string GenerateVerifierCode(int size = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[size];
        rng.GetBytes(randomBytes);
        var verifier = Base64UrlEncode(randomBytes);
        return verifier;
    }

    public string GenerateCodeChallenge(string code_verifier)
    {
        string codeHash64;

        using (SHA256 mySHA256 = SHA256.Create())
        {
            byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(code_verifier));
            codeHash64 = Base64UrlEncode(hashValue);
        }
        return codeHash64;
    }

    private string Base64UrlEncode(byte[] data) =>
    Convert.ToBase64String(data)
        .Replace("+", "-")
        .Replace("/", "_")
        .TrimEnd('=');
}