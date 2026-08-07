namespace PlaylistManagement.Api.DTOs.Common
{
    /// <summary>
    /// Standard error envelope for both Data Annotation validation failures
    /// and unhandled exceptions, so API consumers only ever deal with one
    /// error shape:
    /// { "success": false, "message": "...", "errors": [ { "field", "message" } ] }
    /// </summary>
    public class ApiErrorResponse
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public List<ApiValidationError> Errors { get; set; } = new();
    }

    public class ApiValidationError
    {
        public string Field { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
