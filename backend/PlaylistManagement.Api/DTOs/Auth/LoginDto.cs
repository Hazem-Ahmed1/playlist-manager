using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.DTOs.Auth
{
    /// <summary>
    /// Payload for authenticating an existing account. Only presence/format
    /// is validated here — credential correctness is checked by
    /// SignInManager, not by Data Annotations.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
