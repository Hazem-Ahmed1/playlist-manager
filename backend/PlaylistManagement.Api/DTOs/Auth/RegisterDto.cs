using System.ComponentModel.DataAnnotations;
using PlaylistManagement.Api.Validation;

namespace PlaylistManagement.Api.DTOs.Auth
{
    /// <summary>
    /// Payload for creating a new account. Password strength is enforced by
    /// StrongPasswordAttribute so a violation of any single rule reports its
    /// own specific message, instead of one generic "password is invalid"
    /// error.
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;
    }
}
