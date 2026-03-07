using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kapoot.Application.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace Kapoot.Api.Services;

public static class JwtHelper
{
    public static string GenerateJwt(UserDto user, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "KapootSecretKeyMinimum32Characters!";
        var issuer = config["Jwt:Issuer"] ?? "KapootApi";
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        if (user.IsAdmin)
            claims.Add(new Claim("isAdmin", "true"));
        var token = new JwtSecurityToken(
            issuer,
            issuer,
            claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
