namespace PlaylistManagement.Api.Common
{
    /// <summary>
    /// The set of business-rule failures a service can report through a
    /// Result. Maps 1:1 to an HTTP status code in
    /// ApiControllerBase.FromError, so services describe *what* went wrong
    /// without needing to know anything about HTTP.
    /// </summary>
    public enum ErrorType
    {
        NotFound,
        Forbidden,
        BadRequest,
        Conflict,
        Unauthorized
    }
}
