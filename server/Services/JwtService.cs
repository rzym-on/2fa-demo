using TwoFactorLogin.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TwoFactorLogin.Services;

public static class JwtService
{
    public static string GenerateForUser(User user, AppSettings appSett, bool twoFaToken = false)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(appSett.Secret);

        var claims = new List<Claim>
        {
            new ("username", user.Username),
        };

        var validTo = twoFaToken 
            ? DateTime.UtcNow.AddMinutes(3)
            : DateTime.UtcNow.AddDays(appSett.JwtExpireDays);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = appSett.JwtIssuer,
            Audience = twoFaToken ? appSett.JwtTwoFaAudience : appSett.JwtAudience,
            Subject = new ClaimsIdentity(claims),
            Expires = validTo,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public static User? GetUserFromToken(string token, AppSettings appSett, bool twoFaToken = false)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSett.Secret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = appSett.JwtIssuer,
                ValidAudience = twoFaToken ? appSett.JwtTwoFaAudience : appSett.JwtAudience,
                ClockSkew = TimeSpan.Zero // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken) validatedToken;

            return new User
            {
                Username = jwtToken.Claims.First(x => x.Type == "username").Value
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return null;
    }

    public static string? GetTokenFromRequest(HttpRequest request)
    {
        if (!request.Headers.ContainsKey("Authorization")) return null;

        var authorizationHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader)) return null;

        if (!authorizationHeader.StartsWith("bearer", StringComparison.OrdinalIgnoreCase)) return null;

        return authorizationHeader.Substring("bearer".Length).Trim();
    }
}