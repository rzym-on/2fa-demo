using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OtpNet;
using TwoFactorLogin.Models;
using TwoFactorLogin.Services;

namespace TwoFactorLogin.Controllers;

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

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public ContentResult Register(AuthData data)
    {
        // 1. Sprawdza czy użytkownik istnieje w bazie
        var userDb = JsonDbService.GetUser(data.Username);
        if (userDb != null && userDb.Username  == data.Username)
            throw new Exception("Użytkownk już istnieje w bazie danych");

        // 2. Generuje kody jednorazowe
        var oneTimeCodes = Guid.NewGuid().GetOneTimeCodes();


        // 3. Tworzy nowego użytkownika
        // Hashuje hasło
        // Hashuje kody jednorazowe
        // Tworzy totp-secret
        var user = new User
        {
            Username = data.Username,
            Password = new PasswordHashService(data.Password).ToArray(),
            TotpSecret = CryptoService.GetTotpSecret(),
            OneTimeCodes = oneTimeCodes
                .Select(x => new PasswordHashService(x).ToArray())
                .ToArray()
        };

        // 4. Dodaje użytkownika do bazy
        JsonDbService.AddUser(user);

        // 5. Tworzy totp-uri z którego będie wygenerowany kod QR
        var result = new
        {
            totp = new OtpUri(OtpType.Totp, user.TotpSecret, user.Username, "2fa-demo-app").ToString(),
            oneTimeCodes
        };

        // 6. Zwraca totp-uri i kody jednorazowe
        return new ContentResult
        {
            StatusCode = 200,
            Content = JsonSerializer.Serialize(result)
        };
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public string? Login(AuthData data)
    {
        // 1. Sprawdza, czy użytkownik jest w bazie
        var user = JsonDbService.GetUser(data.Username);
        if (user == null) throw new Exception("Nie ma takiego użytkownika w bazie");

        // 2. Sprawdza, czy zahashowane hasło w bazie jest poprawne z tym w request
        if (!user.ValidatePassword(data.Password)) throw new Exception("Nieprawidłowe hasło");

        // 3. Generuje krótki JWT dla danego użytkownika, który powinien być odesłany z 2 składnikiem
        var shortJwt = JwtService.GenerateForUser(user, _appSettings, twoFaToken:true);

        return shortJwt;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("2fa")]
    public string? Login2Fa([FromBody]string pinCode)
    {
        // 1. Wyciąga token 2fa z requestu
        var token = JwtService.GetTokenFromRequest(Request);
        if (token == null) throw new Exception("Nie ma jwt w zapytaniu");

        // 2. Pobiera usera z tokenu (jeśli token nie jest typem 2fa, to zwróci null)
        var user = JwtService.GetUserFromToken(token, _appSettings, true);
        if (user == null) throw new Exception("Błędny token - brak użytkownia lub token nie jest 2fa");

        // 3. Szuka użytkownika w bazie
        user = JsonDbService.GetUser(user.Username);
        if (user == null) throw new Exception("Brak użytkownika w bazie");

        // 4. Tworzy instancję Totp, żeby serwer wygenerował 6-cyfrowy pin
        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));

        // 5. Serwer sprawdza, czy podany pin zgadza się z wygenerowanym po stronie serwera pinem na bazie TotpSecret
        var isValid = totp.VerifyTotp(pinCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        // 6. Jeśli nie, to sprawdza, czy pin jest kodem jednorazowym
        if (!isValid)
        {
            var usedKey = user.OneTimeCodes
                .FirstOrDefault(x =>
                {
                    PasswordHashService hash = new PasswordHashService(x);
                    return hash.Verify(pinCode);
                });
            if (usedKey == null) return null;

            // 7. Jeśli kod jednorazowy został wykorzystany, usuwamy z bazy
            user.OneTimeCodes = user.OneTimeCodes.Except(new [] { usedKey }).ToArray();
        }

        // 8. Generuje właściwy JWT do pozostałych obszarów systemu
        var jwt = JwtService.GenerateForUser(user, _appSettings);

        return jwt;
    }
}