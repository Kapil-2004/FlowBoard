using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlowBoard.Auth.Services
{
    /// <summary>
    /// Generates and validates JSON Web Tokens.
    /// Configuration is injected via IOptions&lt;JwtSettings&gt; (bound from appsettings.json).
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IOptions<JwtSettings> settings, ILogger<JwtService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public string GenerateToken(User user)
        {
            // ── Build claims payload ─────────────────────────────────────────
            // These claims are accessible from any service that validates the token.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()), // unique token ID
                new Claim(ClaimTypes.NameIdentifier,     user.UserId.ToString()),
                new Claim(ClaimTypes.Email,              user.Email),
                new Claim(ClaimTypes.Name,               user.FullName ?? user.Email),
                new Claim(ClaimTypes.Role,               user.Role)   // Member | BoardAdmin | PlatformAdmin
            };

            // ── Sign with HMAC-SHA256 ────────────────────────────────────────
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _settings.Issuer,
                audience:           _settings.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(_settings.ExpiryHours),
                signingCredentials: creds
            );

            _logger.LogInformation("JWT generated for user {UserId}, expires in {Hours}h",
                                   user.UserId, _settings.ExpiryHours);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc />
        public Guid? GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = _settings.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = _settings.Audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero // no tolerance for expired tokens
                }, out SecurityToken validatedToken);

                var jwt    = (JwtSecurityToken)validatedToken;
                var userIdStr = jwt.Claims
                                   .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
                                   ?.Value;

                return Guid.TryParse(userIdStr, out var userId) ? userId : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}
