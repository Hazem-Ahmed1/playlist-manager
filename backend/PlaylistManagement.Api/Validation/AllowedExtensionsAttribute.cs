using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.Validation
{
    /// <summary>Data Annotation restricting an uploaded IFormFile to a set of allowed extensions (case-insensitive).</summary>
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
            {
                return ValidationResult.Success;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_extensions.Contains(extension))
            {
                return new ValidationResult(
                    ErrorMessage ?? $"Only {string.Join(", ", _extensions)} files are allowed.",
                    new[] { validationContext.MemberName ?? string.Empty });
            }

            return ValidationResult.Success;
        }
    }
}
