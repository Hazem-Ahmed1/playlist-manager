using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.IntegrationTests.Infrastructure;
using Xunit;

namespace PlaylistManagement.IntegrationTests
{
    /// <summary>
    /// Full-pipeline tests for the song catalog: public browsing,
    /// admin-only upload/delete, and file validation, against a real
    /// SQLite in-memory database.
    /// </summary>
    public class SongsControllerTests : IClassFixture<PlaylistApiFactory>
    {
        private readonly PlaylistApiFactory _factory;

        public SongsControllerTests(PlaylistApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> CreateAdminClientAsync()
        {
            var client = _factory.CreateClient();
            var token = await client.LoginAsSeededAdminAsync();
            client.AuthenticateAs(token);
            return client;
        }

        private async Task<HttpClient> CreateRegularUserClientAsync()
        {
            var client = _factory.CreateClient();
            var token = await client.RegisterAndGetTokenAsync();
            client.AuthenticateAs(token);
            return client;
        }

        private static MultipartFormDataContent BuildUploadContent(
            string title = "Test Song",
            string artist = "Test Artist",
            string fileName = "test.mp3",
            string contentType = "audio/mpeg",
            byte[]? bytes = null)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(title), "Title" },
                { new StringContent(artist), "Artist" }
            };

            var fileContent = new ByteArrayContent(bytes ?? "fake audio bytes"u8.ToArray());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "File", fileName);

            return content;
        }

        // Public catalog browsing needs no authentication.
        [Fact]
        public async Task GetAll_AnonymousUser_Returns200OK()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/songs");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // 9. Upload valid MP3. Expected: 201 Created.
        [Fact]
        public async Task Upload_ValidMp3AsAdmin_Returns201Created()
        {
            // Arrange
            var admin = await CreateAdminClientAsync();
            using var content = BuildUploadContent(title: "Valid Upload");

            // Act
            var response = await admin.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<SongDto>>();
            body!.Data!.Title.Should().Be("Valid Upload");
        }

        // 10. Upload unsupported file extension. Expected: 400 Bad Request.
        [Fact]
        public async Task Upload_UnsupportedExtension_Returns400BadRequest()
        {
            // Arrange
            var admin = await CreateAdminClientAsync();
            using var content = BuildUploadContent(fileName: "malware.exe", contentType: "application/octet-stream");

            // Act
            var response = await admin.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>();
            body!.Errors.Should().Contain(e => e.Message.Contains("MP3", StringComparison.OrdinalIgnoreCase));
        }

        // 12. Delete song. Expected: 204 No Content.
        [Fact]
        public async Task Delete_ExistingSongAsAdmin_Returns204NoContent()
        {
            // Arrange
            var admin = await CreateAdminClientAsync();
            using var content = BuildUploadContent(title: "To Delete");
            var uploadResponse = await admin.PostAsync("/api/songs", content);
            var uploaded = await uploadResponse.Content.ReadFromJsonAsync<ApiResponseEnvelope<SongDto>>();

            // Act
            var response = await admin.DeleteAsync($"/api/songs/{uploaded!.Data!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await admin.GetAsync($"/api/songs/{uploaded.Data.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Security: only Admins may add to the catalog. Songs have no
        // per-user owner (they're a shared, admin-managed catalog), so the
        // real analogue of "cannot upload into another user's data" is role
        // enforcement, not ownership.
        [Fact]
        public async Task Upload_AsRegularUser_Returns403Forbidden()
        {
            // Arrange
            var user = await CreateRegularUserClientAsync();
            using var content = BuildUploadContent();

            // Act
            var response = await user.PostAsync("/api/songs", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Security: only Admins may remove catalog songs (the closest real
        // equivalent of "user cannot delete another user's songs").
        [Fact]
        public async Task Delete_AsRegularUser_Returns403Forbidden()
        {
            // Arrange
            var admin = await CreateAdminClientAsync();
            using var content = BuildUploadContent(title: "Protected Song");
            var uploadResponse = await admin.PostAsync("/api/songs", content);
            var uploaded = await uploadResponse.Content.ReadFromJsonAsync<ApiResponseEnvelope<SongDto>>();

            var user = await CreateRegularUserClientAsync();

            // Act
            var response = await user.DeleteAsync($"/api/songs/{uploaded!.Data!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
