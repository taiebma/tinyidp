using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace tinyidp.Encryption;

public class HashedPasswordPbkbf2: IHashedPasswordPbkbf2
{
    private int _iterCount = 100000;
    private int _numBytesRequested = 256 / 8;
    private KeyDerivationPrf _prf = KeyDerivationPrf.HMACSHA256;

    public string GetHashedPasswordPbkbf2(string password)
    {
        int saltSize;

        // Produce a version 3 (see comment above) text hash.
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        saltSize = salt.Length;
        byte[] subkey = KeyDerivation.Pbkdf2(
            password: password, 
            salt: salt, 
            prf: _prf, 
            iterationCount: _iterCount, 
            numBytesRequested: _numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01; // format marker
        WriteNetworkByteOrder(outputBytes, 1, (uint)_prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint)_iterCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
        return Convert.ToBase64String(outputBytes);

    }

    public bool VerifyHashedPasswordPbkbf2(string hashedPassword64, string password)
    {
        int iterCount;
        KeyDerivationPrf prf;
        byte[] hashedPassword = Convert.FromBase64String(hashedPassword64);

        try
        {
            // Read header information
            prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

            // Read the salt: must be >= 128 bits
            if (saltLength < 128 / 8)
            {
                return false;
            }
            if (prf != _prf)
            {
                return false;
            }
            if (iterCount != _iterCount)
            {
                return false;
            }
            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

            // Read the subkey (the rest of the payload): must be >= 128 bits
            int subkeyLength = hashedPassword.Length - 13 - salt.Length;
            if (subkeyLength < 128 / 8)
            {
                return false;
            }
            byte[] expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            // Hash the incoming password and verify it
            byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, _numBytesRequested);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            // This should never occur except in the case of a malformed payload, where
            // we might go off the end of the array. Regardless, a malformed payload
            // implies verification failed.
            return false;
        }
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)(buffer[offset + 0]) << 24)
            | ((uint)(buffer[offset + 1]) << 16)
            | ((uint)(buffer[offset + 2]) << 8)
            | ((uint)(buffer[offset + 3]));
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }



}
