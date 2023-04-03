using TwoFactorLogin.Services;

namespace TwoFactorLogin.Models;

public class User
{
    public string Username { get; set; } = null!;
    public byte[] Password { get; set; } = null!;
    public string? TotpSecret { get; set; }
    public byte[][] OneTimeCodes { get; set; } = null!;

    public bool ValidatePassword(string passwd)
    {
        PasswordHashService hash = new PasswordHashService(Password);
        return hash.Verify(passwd);
    }
}