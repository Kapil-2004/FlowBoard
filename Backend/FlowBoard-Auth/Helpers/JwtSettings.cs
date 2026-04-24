namespace FlowBoard.Auth.Helpers
{
    /// <summary>
    /// Strongly-typed wrapper for JWT configuration values read from appsettings.json.
    /// Bound via options pattern: services.Configure&lt;JwtSettings&gt;(config.GetSection("Jwt"))
    /// </summary>
    public class JwtSettings
    {
        /// <summary>Secret key used to sign the JWT. Must be at least 32 characters.</summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>Token issuer – typically the service URL (e.g., "FlowBoard.Auth").</summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>Intended audience – typically the client app (e.g., "FlowBoard.Client").</summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>Token lifetime in hours. Default: 24.</summary>
        public int ExpiryHours { get; set; } = 24;
    }
}
