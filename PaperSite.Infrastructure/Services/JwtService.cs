using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaperSite.Application.Configuration;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaperSite.Infrastructure.Services;

public class JwtService : IJWtService
{
    private readonly JwtSettings _jwtSettings;

    private const string RegistrationPurpose = "registration";

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role.Name)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryMinutes
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public string GenerateRegistrationToken(string phoneNumber)
    {
        var claims = new List<Claim>
        {
            new("mobile", phoneNumber),
            new("purpose", RegistrationPurpose),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,

            // مهم:
            // Registration Token با AccessToken Audience متفاوت دارد
            audience: $"{_jwtSettings.Audience}.Registration",

            claims: claims,

            // فقط 10 دقیقه
            expires: DateTime.UtcNow.AddMinutes(10),

            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public string? ValidateRegistrationToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)
            );

            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience =
                        $"{_jwtSettings.Audience}.Registration",

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken
            );

            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            if (!string.Equals(
                    jwtToken.Header.Alg,
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.Ordinal))
            {
                return null;
            }

            var purpose =
                principal.FindFirst("purpose")?.Value;

            if (purpose != RegistrationPurpose)
                return null;

            return principal.FindFirst("mobile")?.Value;
        }
        catch
        {
            return null;
        }
    }
}