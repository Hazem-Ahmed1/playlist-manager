namespace PlaylistManagement.Api.Middleware.Exceptions
{
    /// <summary>
    /// Thrown by the service layer when an authenticated user tries to act
    /// on a resource they don't own (e.g. another user's playlist).
    /// Translated by ExceptionHandlingMiddleware into a 403 response.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message) : base(message)
        {
        }
    }
}
