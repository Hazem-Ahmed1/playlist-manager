namespace PlaylistManagement.Api.DTOs.Common
{
    /// <summary>
    /// Standard success envelope carrying a payload:
    /// { "success": true, "message": "...", "data": ... }
    /// Pairs with ApiErrorResponse so every response, success or failure,
    /// has the same top-level shape.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };
    }

    /// <summary>
    /// Standard success envelope with no payload, for actions like delete
    /// that only need to confirm the outcome.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; } = true;

        public string Message { get; set; } = string.Empty;

        public static ApiResponse Ok(string message = "Success") =>
            new() { Success = true, Message = message };
    }
}
