namespace PlaylistManagement.Api.Models.Options
{
    /// <summary>
    /// Strongly-typed JWT settings, bound from the "Jwt" configuration
    /// section via the Options pattern instead of reading IConfiguration
    /// directly wherever a token is needed.
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public int ExpiryMinutes { get; set; } = 60;
    }
}
