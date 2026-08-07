using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>Issues JWTs for authenticated users. Kept separate from IAuthService (SRP): token shape is a distinct concern from credential/account orchestration.</summary>
    public interface ITokenService
    {
        /// <summary>Generates a signed JWT for the given user and roles, along with its expiry.</summary>
        (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
