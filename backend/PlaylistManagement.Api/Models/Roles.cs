namespace PlaylistManagement.Api.Models
{
    /// <summary>Identity role names, centralized so AuthService, DataSeeder, and [Authorize(Roles=...)] attributes never risk a typo'd string diverging.</summary>
    public static class Roles
    {
        public const string Admin = "Admin";

        public const string User = "User";
    }
}
