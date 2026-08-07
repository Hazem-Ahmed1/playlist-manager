namespace PlaylistManagement.IntegrationTests.Infrastructure
{
    /// <summary>Mirrors PlaylistManagement.Api.DTOs.Common.ApiResponse&lt;T&gt; for deserializing responses in tests.</summary>
    public class ApiResponseEnvelope<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }

    /// <summary>Mirrors PlaylistManagement.Api.DTOs.Common.ApiErrorResponse for deserializing failure responses in tests.</summary>
    public class ApiErrorEnvelope
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<ApiValidationErrorItem> Errors { get; set; } = new();
    }

    public class ApiValidationErrorItem
    {
        public string Field { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
