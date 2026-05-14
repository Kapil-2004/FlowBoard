using FlowBoard.Auth.DTOs;

namespace FlowBoard.Auth.Interfaces
{
    /// <summary>
    /// Core authentication operations: register, login, profile management, user search,
    /// and admin-level user management (suspend, reactivate, role change).
    /// </summary>
    public interface IAuthService
    {
        // ── Auth ────────────────────────────────────────────────────────────
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        // ── Profile ─────────────────────────────────────────────────────────
        Task<UserDto> GetProfileAsync(Guid userId);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

        // ── Search ──────────────────────────────────────────────────────────
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query);

        // ── Admin: user management ───────────────────────────────────────────
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<UserDto> ChangeRoleAsync(Guid userId, string newRole);
        Task<UserDto> SuspendUserAsync(Guid userId);
        Task<UserDto> ReactivateUserAsync(Guid userId);
        Task DeleteUserAsync(Guid userId);
    }
}
