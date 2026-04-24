using FlowBoard.Auth.Models;

namespace FlowBoard.Auth.Interfaces
{
    /// <summary>
    /// JWT generation and claims extraction.
    /// Separated from AuthService so it can be mocked independently in tests.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a signed JWT for the given user.
        /// Claims: userId, email, role ("User").
        /// </summary>
        string GenerateToken(User user);

        /// <summary>
        /// Extracts the userId claim from a valid JWT.
        /// Returns null if the token is invalid or expired.
        /// </summary>
        Guid? GetUserIdFromToken(string token);
    }
}
