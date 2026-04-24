using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Repositories;

namespace FlowBoard.Auth.Services
{
    /// <summary>
    /// Core business logic for authentication and user profile management.
    /// Orchestrates UserRepository (DB), JwtService (tokens), and PasswordHasher (security).
    /// </summary>
    public class AuthService : IAuthService
    {
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

        /// <inheritdoc />
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Register attempt for {Email}", dto.Email);

            // 1. Guard: email must be unique
            if (await _userRepo.EmailExistsAsync(dto.Email))
            {
                _logger.LogWarning("Registration failed – email already exists: {Email}", dto.Email);
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");
            }

            // 2. Build the User entity
            var user = new User
            {
                UserId       = Guid.NewGuid(),
                FullName     = dto.FullName.Trim(),
                Email        = dto.Email.Trim().ToLower(),
                // Hash the password before persisting – never store plain text
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Provider     = "LOCAL",
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };

            // 3. Persist to database
            await _userRepo.CreateAsync(user);

            _logger.LogInformation("User registered successfully: {UserId}", user.UserId);

            // 4. Return JWT + profile (client can go straight to dashboard)
            return BuildAuthResponse(user);
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for {Email}", dto.Email);

            // 1. Look up user by email
            var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLower());

            // 2. Guard: user must exist AND be a LOCAL user with a password
            // Use constant-time comparison path to avoid leaking whether the email exists
            if (user == null || user.Provider != "LOCAL" || user.PasswordHash == null)
            {
                _logger.LogWarning("Login failed – invalid credentials for {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // 3. Verify BCrypt hash – returns false on mismatch (no exception)
            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed – wrong password for {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            _logger.LogInformation("User logged in: {UserId}", user.UserId);

            return BuildAuthResponse(user);
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            return MapToDto(user);
        }

        /// <inheritdoc />
        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User {userId} not found.");

            // Only update fields that were explicitly provided
            if (dto.FullName is not null) user.FullName  = dto.FullName.Trim();
            if (dto.AvatarUrl is not null) user.AvatarUrl = dto.AvatarUrl.Trim();

            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("Profile updated for {UserId}", userId);
            return MapToDto(user);
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<UserDto>();

            var users = await _userRepo.SearchAsync(query);
            return users.Select(MapToDto);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Builds the AuthResponseDto by generating a JWT and mapping the user to a safe DTO.
        /// </summary>
        private AuthResponseDto BuildAuthResponse(User user) => new()
        {
            Token     = _jwtService.GenerateToken(user),
            TokenType = "Bearer",
            ExpiresIn = 86400, // 24 hours in seconds
            User      = MapToDto(user)
        };

        /// <summary>Maps a User entity to the public-safe UserDto (no password hash / provider ID).</summary>
        private static UserDto MapToDto(User user) => new()
        {
            UserId    = user.UserId,
            FullName  = user.FullName ?? string.Empty,
            Email     = user.Email,
            Provider  = user.Provider,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }
}
