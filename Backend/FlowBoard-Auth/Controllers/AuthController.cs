using System.Security.Claims;
using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Auth.Controllers
{
    /// <summary>
    /// UC1 – Authentication and User Management Controller
    ///
    /// Endpoints:
    ///   POST   /api/auth/register           – Register with email/password
    ///   POST   /api/auth/login              – Login and receive JWT
    ///   POST   /api/auth/oauth/google       – OAuth via Google access token
    ///   POST   /api/auth/oauth/github       – OAuth via GitHub access token
    ///   GET    /api/auth/profile            – Get own profile (requires JWT)
    ///   PUT    /api/auth/profile            – Update own profile (requires JWT)
    ///   GET    /api/auth/users/search       – Search users (requires JWT)
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService  _authService;
        private readonly IOAuthService _oauthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService         authService,
            IOAuthService        oauthService,
            ILogger<AuthController> logger)
        {
            _authService  = authService;
            _oauthService = oauthService;
            _logger       = logger;
        }

        // ── REGISTER ─────────────────────────────────────────────────────────

        /// <summary>Register a new user with email and password.</summary>
        /// <response code="201">User created – returns JWT and profile</response>
        /// <response code="409">Email is already registered</response>
        /// <response code="400">Validation failed</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail(ModelState.GetErrors()));

            try
            {
                var result = await _authService.RegisterAsync(dto);
                return StatusCode(StatusCodes.Status201Created,
                                  ApiResponse<AuthResponseDto>.Ok("User registered successfully.", result));
            }
            catch (InvalidOperationException ex)
            {
                // Email conflict (business logic)
                return Conflict(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  ApiResponse<string>.Fail("Database connection failed or an internal error occurred. Ensure PostgreSQL is running."));
            }
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        /// <summary>Login with email and password to receive a JWT.</summary>
        /// <response code="200">Login successful – returns JWT and profile</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail(ModelState.GetErrors()));

            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(ApiResponse<AuthResponseDto>.Ok("Login successful.", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ── OAUTH ─────────────────────────────────────────────────────────────

        /// <summary>Login or register via Google access token.</summary>
        /// <response code="200">OAuth successful – returns JWT and profile</response>
        [HttpPost("oauth/google")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleOAuth([FromBody] OAuthRequestDto dto)
        {
            dto.Provider = "GOOGLE"; // enforce provider regardless of body
            return await HandleOAuth(dto);
        }

        /// <summary>Login or register via GitHub access token.</summary>
        /// <response code="200">OAuth successful – returns JWT and profile</response>
        [HttpPost("oauth/github")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GitHubOAuth([FromBody] OAuthRequestDto dto)
        {
            dto.Provider = "GITHUB";
            return await HandleOAuth(dto);
        }

        private async Task<IActionResult> HandleOAuth(OAuthRequestDto dto)
        {
            try
            {
                var result = await _oauthService.HandleOAuthAsync(dto);
                return Ok(ApiResponse<AuthResponseDto>.Ok($"{dto.Provider} login successful.", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<string>.Fail(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <summary>Get the authenticated user's profile.</summary>
        /// <response code="200">Profile returned</response>
        /// <response code="401">Missing or invalid JWT</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.GetProfileAsync(userId.Value);
            return Ok(ApiResponse<UserDto>.Ok("Profile fetched successfully.", profile));
        }

        /// <summary>Update the authenticated user's profile (FullName, AvatarUrl).</summary>
        /// <response code="200">Profile updated</response>
        /// <response code="401">Missing or invalid JWT</response>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var updated = await _authService.UpdateProfileAsync(userId.Value, dto);
            return Ok(ApiResponse<UserDto>.Ok("Profile updated successfully.", updated));
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        /// <summary>Search users by name or email (min 2 chars, max 20 results).</summary>
        /// <param name="q">Search query string</param>
        /// <response code="200">Results (may be empty)</response>
        /// <response code="400">Query too short</response>
        [HttpGet("users/search")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return BadRequest(ApiResponse<string>.Fail("Search query must be at least 2 characters."));

            var users = await _authService.SearchUsersAsync(q);
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok("Search completed.", users));
        }

        // ── HEALTH ────────────────────────────────────────────────────────────

        /// <summary>Health check – used by Docker and the API Gateway.</summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
            => Ok(new { service = "auth-service", status = "healthy", timestamp = DateTime.UtcNow });

        // ── PRIVATE HELPER ────────────────────────────────────────────────────

        /// <summary>
        /// Extracts the userId from the JWT claims.
        /// Returns null when the token is missing or malformed (should not happen after [Authorize]).
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    // ── SHARED API RESPONSE WRAPPER ───────────────────────────────────────────

    /// <summary>
    /// Generic API envelope used by all endpoints.
    /// Gives clients a consistent shape regardless of the operation.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T?     Data    { get; set; }

        public static ApiResponse<T> Ok(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message, Data = default };
    }

    /// <summary>Extension to extract all validation error messages from ModelState.</summary>
    public static class ModelStateExtensions
    {
        public static string GetErrors(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary ms)
            => string.Join("; ", ms.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage));
    }
}
