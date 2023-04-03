namespace TwoFactorLogin;

public class AppSettings
{
    public string Secret { get; set; } = null!;
    public int JwtExpireDays { get; set; }
    public string JwtAudience { get; set; } = null!;
    public string JwtTwoFaAudience { get; set; } = null!;
    public string JwtIssuer { get; set; } = null!;
}