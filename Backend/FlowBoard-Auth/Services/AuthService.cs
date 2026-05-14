using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Repositories;

namespace FlowBoard.Auth.Services
{
    /// <summary>
    /// Core business logic for authentication, user profile management, and admin operations.
    /// Valid roles: Member | BoardAdmin | PlatformAdmin
    /// </summary>
    public class AuthService : IAuthService
    {
        private static readonly HashSet<string> ValidRoles = new()
        {
            "Member", "BoardAdmin", "PlatformAdmin"
        };

        private readonly UserRepository _userRepo;
        private readonly IJwtService    _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserRepository      userRepo,
            IJwtService         jwtService,
            ILogger<AuthService> logger)
        {
            _userRepo   = userRepo;
            _jwtService = jwtService;
            _logger     = logger;
        }

        // ── REGISTER ─────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Register attempt for {Email}", dto.Email);

            if (await _userRepo.EmailExistsAsync(dto.Email))
            {
                _logger.LogWarning("Registration failed – email already exists: {Email}", dto.Email);
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");
            }

            var user = new User
            {
                UserId       = Guid.NewGuid(),
                FullName     = dto.FullName.Trim(),
                Email        = dto.Email.Trim().ToLower(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Provider     = "LOCAL",
                Role         = "Member",      // new accounts start as Member
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };

            await _userRepo.CreateAsync(user);
            _logger.LogInformation("User registered: {UserId}", user.UserId);
            return BuildAuthResponse(user);
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for {Email}", dto.Email);

            var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLower());

            if (user == null || user.Provider != "LOCAL" || user.PasswordHash == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is suspended. Contact a platform administrator.");

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Track last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("User logged in: {UserId} Role={Role}", user.UserId, user.Role);
            return BuildAuthResponse(user);
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");
            return MapToDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            if (dto.FullName  is not null) user.FullName  = dto.FullName.Trim();
            if (dto.AvatarUrl is not null) user.AvatarUrl = dto.AvatarUrl.Trim();

            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("Profile updated for {UserId}", userId);
            return MapToDto(user);
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<UserDto>();

            var users = await _userRepo.SearchAsync(query);
            return users.Select(MapToDto);
        }

        // ── ADMIN: USER MANAGEMENT ────────────────────────────────────────────

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");
            return MapToDto(user);
        }

        public async Task<UserDto> ChangeRoleAsync(Guid userId, string newRole)
        {
            if (!ValidRoles.Contains(newRole))
                throw new ArgumentException($"Invalid role '{newRole}'. Valid roles: {string.Join(", ", ValidRoles)}");

            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            // Protect the seed admin from role demotion
            if (user.UserId == new Guid("00000000-0000-0000-0000-000000000001")
                && newRole != "PlatformAdmin")
                throw new InvalidOperationException("Cannot change the role of the primary platform admin.");

            user.Role = newRole;
            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("Role changed for {UserId} → {Role}", userId, newRole);
            return MapToDto(user);
        }

        public async Task<UserDto> SuspendUserAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            if (user.UserId == new Guid("00000000-0000-0000-0000-000000000001"))
                throw new InvalidOperationException("Cannot suspend the primary platform admin.");

            user.IsActive = false;
            await _userRepo.UpdateAsync(user);
            _logger.LogWarning("User suspended: {UserId}", userId);
            return MapToDto(user);
        }

        public async Task<UserDto> ReactivateUserAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.IsActive = true;
            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("User reactivated: {UserId}", userId);
            return MapToDto(user);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            if (userId == new Guid("00000000-0000-0000-0000-000000000001"))
                throw new InvalidOperationException("Cannot delete the primary platform admin.");

            await _userRepo.DeleteAsync(userId);
            _logger.LogWarning("User deleted: {UserId}", userId);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private AuthResponseDto BuildAuthResponse(User user) => new()
        {
            Token     = _jwtService.GenerateToken(user),
            TokenType = "Bearer",
            ExpiresIn = 86400,
            User      = MapToDto(user)
        };

        private static UserDto MapToDto(User user) => new()
        {
            UserId      = user.UserId,
            FullName    = user.FullName ?? string.Empty,
            Email       = user.Email,
            Provider    = user.Provider,
            AvatarUrl   = user.AvatarUrl,
            Role        = user.Role,
            IsActive    = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt   = user.CreatedAt
        };
    }
}
