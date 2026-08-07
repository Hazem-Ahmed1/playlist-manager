using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlaylistManagement.Api.DTOs.Auth;
using PlaylistManagement.IntegrationTests.Infrastructure;
using Xunit;

namespace PlaylistManagement.IntegrationTests
{
    /// <summary>
    /// Full-pipeline tests for registration and login against a real SQLite
    /// in-memory database — routing, Data Annotation validation, Identity,
    /// and JWT issuance all run for real, nothing is mocked.
    /// </summary>
    public class AuthControllerTests : IClassFixture<PlaylistApiFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(PlaylistApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        // 1. Register user. Expected: 201 Created.
        [Fact]
        public async Task Register_NewUser_Returns201Created()
        {
            // Arrange
            var request = new
            {
                firstName = "Ada",
                lastName = "Lovelace",
                email = $"ada-{Guid.NewGuid():N}@example.com",
                password = "Str0ng!Pass"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert: HTTP 201, and the body carries a usable JWT.
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<AuthResponseDto>>();
            body!.Success.Should().BeTrue();
            body.Data!.Token.Should().NotBeNullOrWhiteSpace();
            body.Data.Email.Should().Be(request.email);
        }

        // 2. Login. Expected: 200 OK, JWT returned.
        [Fact]
        public async Task Login_ValidCredentials_Returns200OkWithJwt()
        {
            // Arrange
            var email = $"login-{Guid.NewGuid():N}@example.com";
            const string password = "Str0ng!Pass";
            await _client.RegisterAndGetTokenAsync(email, password);

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<AuthResponseDto>>();
            body!.Data!.Token.Should().NotBeNullOrWhiteSpace();
        }

        // Negative case: registering the same email twice is rejected.
        [Fact]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            // Arrange
            var email = $"dup-{Guid.NewGuid():N}@example.com";
            var request = new { firstName = "Ada", lastName = "Lovelace", email, password = "Str0ng!Pass" };
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Act: register the exact same email again.
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        // Negative case: wrong password is rejected without revealing which part was wrong.
        [Fact]
        public async Task Login_WrongPassword_Returns401Unauthorized()
        {
            // Arrange
            var email = $"wrongpw-{Guid.NewGuid():N}@example.com";
            await _client.RegisterAndGetTokenAsync(email, "Correct1!Pass");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Incorrect1!Pass" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
