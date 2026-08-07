using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.Validation
{
    /// <summary>
    /// Data Annotation attribute enforcing password strength: 8+ characters
    /// with at least one uppercase letter, one lowercase letter, one digit,
    /// and one special character. Reports the first rule that fails with
    /// its own specific message, per the spec's distinct error list.
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string ?? string.Empty;

            // Emptiness is [Required]'s job; don't double-report it here.
            if (string.IsNullOrEmpty(password))
            {
                return ValidationResult.Success;
            }

            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters.", new[] { validationContext.MemberName ?? string.Empty });
            }

            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("Password must contain an uppercase letter.", new[] { validationContext.MemberName ?? string.Empty });
            }

            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("Password must contain a lowercase letter.", new[] { validationContext.MemberName ?? string.Empty });
            }

            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Password must contain a number.", new[] { validationContext.MemberName ?? string.Empty });
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ValidationResult("Password must contain a special character.", new[] { validationContext.MemberName ?? string.Empty });
            }

            return ValidationResult.Success;
        }
    }
}
