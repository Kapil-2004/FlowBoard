using System.ComponentModel.DataAnnotations;

namespace FlowBoard.Auth.DTOs
{
    /// <summary>
    /// Request body for POST /api/auth/register
    /// </summary>
    public class RegisterDto
    {
        /// <summary>User's display name.</summary>
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Valid email address – must be unique in the system.</summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password with minimum security requirements.
        /// Stored as BCrypt hash – never stored in plain text.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
