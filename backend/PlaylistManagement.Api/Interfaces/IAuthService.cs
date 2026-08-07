using PlaylistManagement.Api.DTOs.Auth;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>Orchestrates account creation and credential verification via ASP.NET Core Identity, then issues a JWT.</summary>
    public interface IAuthService
    {
        /// <summary>
        /// Creates a new account. Throws ConflictException if the email is
        /// already registered, BadRequestException if Identity rejects the
        /// password/user for any other reason.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Verifies credentials and issues a token. Throws
        /// UnauthorizedAccessException if the email/password don't match.
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
