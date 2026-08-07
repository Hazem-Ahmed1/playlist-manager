using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PlaylistManagement.Api.Controllers
{
    /// <summary>
    /// Shared base for authenticated controllers. Centralizes reading the
    /// current user's id from JWT claims so individual controllers stay
    /// thin and don't repeat claim-parsing logic.
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>The authenticated user's id (from the JWT "sub"/NameIdentifier claim).</summary>
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("The current user could not be identified.");
    }
}
