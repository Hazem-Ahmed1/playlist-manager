using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Auth;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>Orchestrates account creation and credential verification via ASP.NET Core Identity, then issues a JWT.</summary>
    public interface IAuthService
    {
        /// <summary>
        /// Creates a new account. Fails with Conflict if the email is
        /// already registered, BadRequest if Identity rejects the
        /// password/user for any other reason.
        /// </summary>
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Verifies credentials and issues a token. Fails with Unauthorized
        /// if the email/password don't match — wrong credentials are an
        /// expected outcome, reported as a normal error, not an exception.
        /// </summary>
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}
