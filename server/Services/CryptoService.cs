using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace TwoFactorLogin.Services;

// W podziękowaniu za ładne hashowanie
// https://jonathancrozier.com/blog/how-to-generate-a-cryptographically-secure-random-string-in-dot-net-with-c-sharp
public static class CryptoService
{
    public static string[] GetOneTimeCodes(this Guid guid)
    {
        var guidSha = guid.ToString().ToSha512();
        var len = 5;
        var arr = Enumerable.Range(0, 10)
            .Select(x => guidSha.Substring(x * len, len))
            .ToArray();

        return arr;
    }

    public static string GetTotpSecret()
    {
        var guidBytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString().ToSha512());

        return Base32Encoding.ToString(guidBytes);
    }

    public static string ToSha512(this string value)
    {
        using var sha = SHA512.Create();
    
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash  = sha.ComputeHash(bytes);
 
        return Convert.ToBase64String(hash);
    }
}