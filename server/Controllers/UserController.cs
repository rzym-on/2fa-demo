using TwoFactorLogin.Models;
using TwoFactorLogin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OtpNet;
using System.Text.Json;
using System.Text;

namespace TwoFactorLogin.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    public record AuthData(string Username, string Password);
    private readonly AppSettings _appSettings;
    public UserController(IOptions<AppSettings> settings)
    {
        _appSettings = settings.Value;
    }

    [HttpPost]
    [Route("register")]
    public ContentResult Register(AuthData data)
    {
        var userDb = JsonDbService.GetUser(data.Username);
        if (userDb != null && userDb.Username  == data.Username)
            throw new Exception("Użytkownk już istnieje w bazie danych");

        var oneTimeCodes = Guid.NewGuid().GetOneTimeCodes();

        var user = new User
        {
            Username = data.Username,
            Password = new PasswordHashService(data.Password).ToArray(),
            TotpSecret = CryptoService.GetTotpSecret(),
            // Hash oneTimeKeys in Db
            OneTimeCodes = oneTimeCodes
                .Select(x => new PasswordHashService(x).ToArray())
                .ToArray()
        };

        JsonDbService.AddUser(user);

        var result = new
        {
            totp = new OtpUri(OtpType.Totp, user.TotpSecret, user.Username, "2fa-demo-app").ToString(),
            oneTimeCodes
        };

        return new ContentResult
        {
            StatusCode = 200,
            Content = JsonSerializer.Serialize(result)
        };
    }

    [HttpPost]
    [Route("login")]
    public string? Login(AuthData data)
    {
        var user = JsonDbService.GetUser(data.Username);
        if (user == null) throw new Exception("Nie ma takiego użytkownika w bazie");

        if (!user.ValidatePassword(data.Password)) throw new Exception("Nieprawidłowe hasło");

        var shortJwt = JwtService.GenerateForUser(user, _appSettings, true);

        return shortJwt;
    }

    [HttpPost]
    [Route("2fa")]
    public string? Login2Fa([FromBody]string pinCode)
    {
        var token = JwtService.GetTokenFromRequest(Request);
        if (token == null) throw new Exception("Nie ma jwt w zapytaniu");

        var user = JwtService.GetUserFromToken(token, _appSettings, true);
        if (user == null) throw new Exception("Błędny token - brak użytkownia lub token nie jest 2fa");

        user = JsonDbService.GetUser(user.Username);
        if (user == null) throw new Exception("Brak użytkownika w bazie");

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        var isValid = totp.VerifyTotp(pinCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        if (!isValid)
        {
            var usedKey = user.OneTimeCodes
                .FirstOrDefault(x =>
                {
                    PasswordHashService hash = new PasswordHashService(x);
                    return hash.Verify(pinCode);
                });
            if (usedKey == null) return null;

            user.OneTimeCodes = user.OneTimeCodes.Except(new [] { usedKey }).ToArray();
        }

        var jwt = JwtService.GenerateForUser(user, _appSettings);

        return jwt;
    }
}