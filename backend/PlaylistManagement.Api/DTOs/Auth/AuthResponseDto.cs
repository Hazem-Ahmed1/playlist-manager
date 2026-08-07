namespace PlaylistManagement.Api.DTOs.Auth
{
    /// <summary>Returned after a successful register or login.</summary>
    public class AuthResponseDto
    {
        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}
