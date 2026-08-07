using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.Validation
{
    /// <summary>Data Annotation restricting an uploaded IFormFile to a maximum size, in bytes.</summary>
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxBytes;

        public MaxFileSizeAttribute(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
            {
                return ValidationResult.Success;
            }

            if (file.Length > _maxBytes)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"File size cannot exceed {_maxBytes / (1024 * 1024)} MB.",
                    new[] { validationContext.MemberName ?? string.Empty });
            }

            return ValidationResult.Success;
        }
    }
}
