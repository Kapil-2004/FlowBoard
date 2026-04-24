using FlowBoard.Auth.DTOs;

namespace FlowBoard.Auth.Interfaces
{
    /// <summary>
    /// Handles OAuth2 flow for Google and GitHub.
    /// Validates the provider access token server-side, then creates or retrieves the user.
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Validates the access token with the provider's userinfo endpoint,
        /// creates a new user if first login, then returns a JWT.
        /// </summary>
        Task<AuthResponseDto> HandleOAuthAsync(OAuthRequestDto dto);
    }
}
