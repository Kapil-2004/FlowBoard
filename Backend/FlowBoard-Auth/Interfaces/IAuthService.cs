using FlowBoard.Auth.DTOs;

namespace FlowBoard.Auth.Interfaces
{
    /// <summary>
    /// Core authentication operations: register, login, profile management, and user search.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with email/password.
        /// Throws InvalidOperationException if email is already taken.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Validates credentials and returns a JWT.
        /// Throws UnauthorizedAccessException on invalid credentials.
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Returns the public profile of the currently authenticated user.
        /// </summary>
        Task<UserDto> GetProfileAsync(Guid userId);

        /// <summary>
        /// Updates mutable profile fields (FullName, AvatarUrl).
        /// Returns the updated profile.
        /// </summary>
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

        /// <summary>
        /// Full-text search across FullName and Email.
        /// Used by other services (e.g. workspace member search).
        /// </summary>
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query);
    }
}
