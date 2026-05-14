using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Auth.Models
{
    /// <summary>
    /// Represents a user account in FlowBoard.
    /// Supports both local (email/password) and OAuth (Google/GitHub) login.
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>Primary key – UUID generated on creation.</summary>
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; } = Guid.NewGuid();

        /// <summary>Display name of the user.</summary>
        [Column("full_name")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        /// <summary>Unique email address – required for all users.</summary>
        [Column("email")]
        [Required]
        [MaxLength(320)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// BCrypt-hashed password. Null for OAuth-only users.
        /// </summary>
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Authentication provider: LOCAL | GOOGLE | GITHUB
        /// </summary>
        [Column("provider")]
        [MaxLength(20)]
        public string Provider { get; set; } = "LOCAL";

        /// <summary>
        /// Provider-specific user ID. Null for local users.
        /// Used to match returning OAuth users without creating duplicates.
        /// </summary>
        [Column("provider_id")]
        [MaxLength(200)]
        public string? ProviderId { get; set; }

        /// <summary>Profile picture URL from OAuth provider or user upload.</summary>
        [Column("avatar_url")]
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Platform role: Member | BoardAdmin | PlatformAdmin
        /// Determines access level across all services.
        /// </summary>
        [Column("role")]
        [MaxLength(30)]
        public string Role { get; set; } = "Member";

        /// <summary>Whether the account is active. Suspended users cannot log in.</summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>UTC timestamp of last successful login.</summary>
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>UTC timestamp of account creation.</summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of last profile update.</summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
