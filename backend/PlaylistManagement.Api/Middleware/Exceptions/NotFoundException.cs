namespace PlaylistManagement.Api.Middleware.Exceptions
{
    /// <summary>
    /// Thrown by the service layer when a requested entity does not exist.
    /// Translated by ExceptionHandlingMiddleware into a 404 response.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
