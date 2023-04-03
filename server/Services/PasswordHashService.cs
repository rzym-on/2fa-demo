using System.Security.Cryptography;

namespace TwoFactorLogin.Services;

// W podziękowaniu za hashowanie
// https://stackoverflow.com/questions/4181198/how-to-hash-a-password
// http://csharptest.net/470/another-example-of-how-to-store-a-salted-password-hash/

public sealed class PasswordHashService
{
    const int SaltSize = 16, HashSize = 20, HashIter = 10000;
    readonly byte[] _salt, _hash;
    // public byte[] Salt => (byte[])_salt.Clone();
    // public byte[] Hash => (byte[])_hash.Clone();

    public PasswordHashService(string password)
    {
        // new RNGCryptoServiceProvider().GetBytes(_salt = new byte[SaltSize]);
        // _hash = new Rfc2898DeriveBytes(password, _salt, HashIter).GetBytes(HashSize);
        _salt = RandomNumberGenerator.GetBytes(SaltSize);
        _hash = new Rfc2898DeriveBytes(password, _salt, HashIter).GetBytes(HashSize);
    }

    public PasswordHashService(byte[] hashBytes)
    {
        Array.Copy(hashBytes, 0, _salt = new byte[SaltSize], 0, SaltSize);
        Array.Copy(hashBytes, SaltSize, _hash = new byte[HashSize], 0, HashSize);
    }

    public PasswordHashService(byte[] salt, byte[] hash)
    {
        Array.Copy(salt, 0, _salt = new byte[SaltSize], 0, SaltSize);
        Array.Copy(hash, 0, _hash = new byte[HashSize], 0, HashSize);
    }

    public byte[] ToArray()
    {
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(_salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(_hash, 0, hashBytes, SaltSize, HashSize);
        return hashBytes;
    }

    public bool Verify(string password)
    {
        byte[] test = new Rfc2898DeriveBytes(password, _salt, HashIter).GetBytes(HashSize);
        for (int i = 0; i < HashSize; i++)
            if (test[i] != _hash[i])
                return false;
        return true;
    }
}