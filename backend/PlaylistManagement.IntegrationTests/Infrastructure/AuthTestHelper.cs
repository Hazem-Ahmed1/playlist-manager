using System.Net.Http.Headers;
using System.Net.Http.Json;
using PlaylistManagement.Api.DTOs.Auth;

namespace PlaylistManagement.IntegrationTests.Infrastructure
{
    /// <summary>Convenience extensions so tests don't hand-roll register/login HTTP calls.</summary>
    public static class AuthTestHelper
    {
        public const string SeededAdminEmail = "admin@playlist.local";
        public const string SeededAdminPassword = "Admin@12345";

        public static async Task<string> RegisterAndGetTokenAsync(
            this HttpClient client,
            string? email = null,
            string password = "P@ssw0rd1",
            string firstName = "Test",
            string lastName = "User")
        {
            email ??= $"user-{Guid.NewGuid():N}@example.com";

            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<AuthResponseDto>>();
            return body!.Data!.Token;
        }

        public static async Task<string> LoginAndGetTokenAsync(this HttpClient client, string email, string password)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<AuthResponseDto>>();
            return body!.Data!.Token;
        }

        public static Task<string> LoginAsSeededAdminAsync(this HttpClient client) =>
            client.LoginAndGetTokenAsync(SeededAdminEmail, SeededAdminPassword);

        public static void AuthenticateAs(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
