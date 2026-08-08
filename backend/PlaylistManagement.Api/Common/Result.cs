namespace PlaylistManagement.Api.Common
{
    /// <summary>
    /// Outcome of a service operation that returns a value. Services return
    /// this instead of throwing for expected business-rule failures (not
    /// found, forbidden, duplicate name, wrong credentials, ...) so the only
    /// exceptions that can still reach ExceptionHandlingMiddleware are
    /// genuinely unexpected ones (a DB failure, a bug), not normal control
    /// flow.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }

        public T? Value { get; }

        public string? ErrorMessage { get; }

        public ErrorType ErrorType { get; }

        private Result(bool isSuccess, T? value, string? errorMessage, ErrorType errorType)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static Result<T> Success(T value) => new(true, value, null, default);

        public static Result<T> Failure(ErrorType errorType, string errorMessage) =>
            new(false, default, errorMessage, errorType);
    }

    /// <summary>Same as <see cref="Result{T}"/>, for operations with no value to return.</summary>
    public class Result
    {
        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        public ErrorType ErrorType { get; }

        private Result(bool isSuccess, string? errorMessage, ErrorType errorType)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static Result Success() => new(true, null, default);

        public static Result Failure(ErrorType errorType, string errorMessage) =>
            new(false, errorMessage, errorType);
    }
}
