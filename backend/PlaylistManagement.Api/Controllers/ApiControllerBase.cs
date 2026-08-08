using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Common;

namespace PlaylistManagement.Api.Controllers
{
    /// <summary>
    /// Shared base for authenticated controllers. Centralizes reading the
    /// current user's id from JWT claims, and translating a failed Result
    /// into its matching HTTP response, so individual controllers stay thin
    /// and don't repeat either.
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>The authenticated user's id (from the JWT "sub"/NameIdentifier claim).</summary>
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("The current user could not be identified.");

        /// <summary>
        /// Turns a failed Result/Result&lt;T&gt; into the matching HTTP
        /// response — the one place that translates a service-layer
        /// ErrorType into a status code, so actions never switch on it
        /// themselves.
        /// </summary>
        protected IActionResult FromError(ErrorType errorType, string message)
        {
            var response = new ApiErrorResponse { Message = message };

            return errorType switch
            {
                ErrorType.NotFound => NotFound(response),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                ErrorType.BadRequest => BadRequest(response),
                ErrorType.Conflict => Conflict(response),
                ErrorType.Unauthorized => Unauthorized(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
