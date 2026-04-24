using System.ComponentModel.DataAnnotations;

namespace FlowBoard.Auth.DTOs
{
    /// <summary>
    /// Request body for POST /api/auth/login
    /// </summary>
    public class LoginDto
    {
        /// <summary>Registered email address.</summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Plain-text password (validated against BCrypt hash).</summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
