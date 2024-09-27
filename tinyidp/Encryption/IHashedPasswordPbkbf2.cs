using System;

namespace tinyidp.Encryption;

public interface IHashedPasswordPbkbf2
{
    public string GetHashedPasswordPbkbf2(string password);
    public bool VerifyHashedPasswordPbkbf2(string hashedPassword64, string password);

}
