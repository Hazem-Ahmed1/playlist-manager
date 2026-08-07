using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PlaylistManagement.IntegrationTests.Infrastructure;
using Xunit;

namespace PlaylistManagement.IntegrationTests
{
    /// <summary>
    /// End-to-end verification that Data Annotation validation on request
    /// DTOs actually reaches callers as 400 responses in the
    /// { success, message, errors: [{ field, message }] } envelope, with
    /// the spec's exact custom error messages — not just that the model
    /// binder rejects the request.
    /// </summary>
    public class ValidationTests : IClassFixture<PlaylistApiFactory>
    {
        private readonly PlaylistApiFactory _factory;

        public ValidationTests(PlaylistApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient();
            var token = await client.RegisterAndGetTokenAsync();
            client.AuthenticateAs(token);
            return client;
        }

        // Missing playlist name.
        [Fact]
        public async Task CreatePlaylist_MissingName_Returns400WithCustomMessage()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();

            // Act
            var response = await client.PostAsJsonAsync("/api/playlists", new { name = "" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Message.Should().Be("Validation Failed");
            body.Errors.Should().Contain(e => e.Message == "Playlist name is required.");
        }

        // Empty song title.
        [Fact]
        public async Task UploadSong_EmptyTitle_Returns400WithCustomMessage()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await client.LoginAsSeededAdminAsync();
            client.AuthenticateAs(token);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(""), "Title" },
                { new StringContent("Some Artist"), "Artist" }
            };
            var fileContent = new ByteArrayContent("audio bytes"u8.ToArray());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            content.Add(fileContent, "File", "song.mp3");

            // Act
            var response = await client.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == "Song title is required.");
        }

        // Invalid email.
        [Fact]
        public async Task Register_InvalidEmail_Returns400WithCustomMessage()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Ada",
                lastName = "Lovelace",
                email = "not-an-email",
                password = "Str0ng!Pass"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == "Invalid email address.");
        }

        // Weak password.
        [Theory]
        [InlineData("short1!", "Password must be at least 8 characters.")]
        [InlineData("alllowercase1!", "Password must contain an uppercase letter.")]
        [InlineData("ALLUPPERCASE1!", "Password must contain a lowercase letter.")]
        [InlineData("NoDigitsHere!", "Password must contain a number.")]
        [InlineData("NoSpecialChars1", "Password must contain a special character.")]
        public async Task Register_WeakPassword_Returns400WithSpecificRuleMessage(string weakPassword, string expectedMessage)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Ada",
                lastName = "Lovelace",
                email = $"weakpw-{Guid.NewGuid():N}@example.com",
                password = weakPassword
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == expectedMessage);
        }

        // Missing file.
        [Fact]
        public async Task UploadSong_MissingFile_Returns400BadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await client.LoginAsSeededAdminAsync();
            client.AuthenticateAs(token);

            using var content = new MultipartFormDataContent
            {
                { new StringContent("A Title"), "Title" },
                { new StringContent("An Artist"), "Artist" }
            };
            // Deliberately no "File" part.

            // Act
            var response = await client.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == "Song file is required.");
        }

        // File too large.
        [Fact]
        public async Task UploadSong_FileExceeds20MB_Returns400WithCustomMessage()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await client.LoginAsSeededAdminAsync();
            client.AuthenticateAs(token);

            using var content = new MultipartFormDataContent
            {
                { new StringContent("Huge Song"), "Title" },
                { new StringContent("Some Artist"), "Artist" }
            };
            var oversized = new byte[20 * 1024 * 1024 + 1];
            var fileContent = new ByteArrayContent(oversized);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            content.Add(fileContent, "File", "huge.mp3");

            // Act
            var response = await client.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == "File size cannot exceed 20 MB.");
        }

        // Invalid file extension.
        [Fact]
        public async Task UploadSong_InvalidExtension_Returns400WithCustomMessage()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await client.LoginAsSeededAdminAsync();
            client.AuthenticateAs(token);

            using var content = new MultipartFormDataContent
            {
                { new StringContent("Bad Extension Song"), "Title" },
                { new StringContent("Some Artist"), "Artist" }
            };
            var fileContent = new ByteArrayContent("not audio"u8.ToArray());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(fileContent, "File", "notes.txt");

            // Act
            var response = await client.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message == "Only MP3, WAV, and M4A files are allowed.");
        }
    }
}
