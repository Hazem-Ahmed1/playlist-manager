namespace PlaylistManagement.Api.Middleware.Exceptions
{
    /// <summary>
    /// Thrown by the service layer when a request conflicts with existing
    /// state (e.g. registering an email that's already taken). Translated
    /// into a 409 response.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
