namespace PlaylistManagement.Api.Middleware.Exceptions
{
    /// <summary>
    /// Thrown by the service layer when a request violates a business rule
    /// that isn't expressible as a Data Annotation (e.g. a song that's
    /// already in the playlist). Translated into a 400 response.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
