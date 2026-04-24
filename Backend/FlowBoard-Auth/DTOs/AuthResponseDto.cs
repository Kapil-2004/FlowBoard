namespace FlowBoard.Auth.DTOs
{
    /// <summary>
    /// Returned by Register, Login, and OAuth endpoints.
    /// Contains the JWT token and basic user info for the client to bootstrap the session.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>JWT Bearer token – valid for 24 hours.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Token type, always "Bearer".</summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>Token expiry in seconds from now (86400 = 24 h).</summary>
        public int ExpiresIn { get; set; } = 86400;

        /// <summary>Lightweight user profile embedded so the client doesn't need a follow-up request.</summary>
        public UserDto User { get; set; } = null!;
    }
}
