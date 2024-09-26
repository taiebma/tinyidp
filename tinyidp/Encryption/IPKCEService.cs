namespace tinyidp.Encryption;

public interface IPKCEService
{
    public bool ValidPKCE(string code_verifier, string CodeChallenge);
    public string GenerateVerifierCode(int size = 32);
    public string GenerateCodeChallenge(string code_verifier);

}