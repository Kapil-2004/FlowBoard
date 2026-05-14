using System.ComponentModel.DataAnnotations;

namespace FlowBoard.Auth.DTOs
{
    /// <summary>
    /// Safe user representation – never exposes PasswordHash or ProviderId.
    /// Used in GET /api/auth/profile responses and search results.
    /// </summary>
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = "Member";
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request body for PUT /api/auth/profile
    /// All fields optional – only non-null values are applied.
    /// </summary>
    public class UpdateProfileDto
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Request body for OAuth login/registration.
    /// Carries the provider's access token for server-side token exchange.
    /// </summary>
    public class OAuthRequestDto
    {
        [Required]
        public string Provider { get; set; } = string.Empty;   // "GOOGLE" | "GITHUB"

        [Required]
        public string AccessToken { get; set; } = string.Empty; // Token from OAuth provider
    }
}
