using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.IntegrationTests.Infrastructure;
using Xunit;

namespace PlaylistManagement.IntegrationTests
{
    /// <summary>
    /// Full-pipeline tests for playlist CRUD, ownership enforcement, and
    /// per-user isolation, against a real SQLite in-memory database.
    /// </summary>
    public class PlaylistsControllerTests : IClassFixture<PlaylistApiFactory>
    {
        private readonly PlaylistApiFactory _factory;

        public PlaylistsControllerTests(PlaylistApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> CreateAuthenticatedClientAsync(string? email = null)
        {
            var client = _factory.CreateClient();
            var token = await client.RegisterAndGetTokenAsync(email);
            client.AuthenticateAs(token);
            return client;
        }

        // 3. Authenticated user creates playlist. Expected: 201 Created.
        [Fact]
        public async Task Create_AuthenticatedUser_Returns201Created()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();

            // Act
            var response = await client.PostAsJsonAsync("/api/playlists", new { name = "Road Trip", description = "Driving songs" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<PlaylistDto>>();
            body!.Data!.Name.Should().Be("Road Trip");
        }

        // 4. Anonymous user creates playlist. Expected: 401 Unauthorized.
        [Fact]
        public async Task Create_AnonymousUser_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/playlists", new { name = "Should Fail" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // 5. Authenticated user gets only their own playlists. Expected: 200 OK.
        [Fact]
        public async Task GetMyPlaylists_UserHasPlaylists_ReturnsOnlyOwnPlaylists()
        {
            // Arrange
            var userA = await CreateAuthenticatedClientAsync();
            var userB = await CreateAuthenticatedClientAsync();

            await userA.PostAsJsonAsync("/api/playlists", new { name = "A's Gym Mix" });
            await userA.PostAsJsonAsync("/api/playlists", new { name = "A's Chill Mix" });
            await userB.PostAsJsonAsync("/api/playlists", new { name = "B's Party Mix" });

            // Act
            var response = await userA.GetAsync("/api/playlists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<PlaylistDto>>>();
            body!.Data.Should().HaveCount(2);
            body.Data!.Select(p => p.Name).Should().BeEquivalentTo("A's Gym Mix", "A's Chill Mix");
        }

        // 7. Update playlist. Expected: 200 OK.
        [Fact]
        public async Task Update_OwnPlaylist_Returns200OkWithUpdatedData()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            var created = await CreateAndReadPlaylistAsync(client, "Original Name");

            // Act
            var response = await client.PutAsJsonAsync($"/api/playlists/{created.Id}", new { name = "Renamed", description = "Updated" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<PlaylistDto>>();
            body!.Data!.Name.Should().Be("Renamed");
        }

        // 8. Delete playlist. Expected: 204 No Content.
        [Fact]
        public async Task Delete_OwnPlaylist_Returns204NoContent()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            var created = await CreateAndReadPlaylistAsync(client, "To Delete");

            // Act
            var response = await client.DeleteAsync($"/api/playlists/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await client.GetAsync($"/api/playlists/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Security: user cannot modify another user's playlist.
        [Fact]
        public async Task Update_AnotherUsersPlaylist_Returns403Forbidden()
        {
            // Arrange
            var owner = await CreateAuthenticatedClientAsync();
            var intruder = await CreateAuthenticatedClientAsync();
            var created = await CreateAndReadPlaylistAsync(owner, "Owner's Playlist");

            // Act
            var response = await intruder.PutAsJsonAsync($"/api/playlists/{created.Id}", new { name = "Hijacked" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Security: deleting another user's playlist is equally forbidden.
        [Fact]
        public async Task Delete_AnotherUsersPlaylist_Returns403Forbidden()
        {
            // Arrange
            var owner = await CreateAuthenticatedClientAsync();
            var intruder = await CreateAuthenticatedClientAsync();
            var created = await CreateAndReadPlaylistAsync(owner, "Owner's Playlist");

            // Act
            var response = await intruder.DeleteAsync($"/api/playlists/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Security: adding a song to another user's playlist is forbidden
        // (the closest real equivalent to the spec's "upload songs into
        // another user's playlist" — songs are a shared admin-managed
        // catalog, so the user-facing action is attaching an existing
        // catalog song to a playlist, not uploading a file into one).
        [Fact]
        public async Task AddSong_ToAnotherUsersPlaylist_Returns403Forbidden()
        {
            // Arrange
            var owner = await CreateAuthenticatedClientAsync();
            var intruder = await CreateAuthenticatedClientAsync();
            var created = await CreateAndReadPlaylistAsync(owner, "Owner's Playlist");

            var admin = _factory.CreateClient();
            var adminToken = await admin.LoginAsSeededAdminAsync();
            admin.AuthenticateAs(adminToken);
            var song = await UploadTestSongAsync(admin);

            // Act
            var response = await intruder.PostAsJsonAsync($"/api/playlists/{created.Id}/songs", new { songId = song.Id });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static async Task<PlaylistDto> CreateAndReadPlaylistAsync(HttpClient client, string name)
        {
            var response = await client.PostAsJsonAsync("/api/playlists", new { name });
            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<PlaylistDto>>();
            return body!.Data!;
        }

        internal static async Task<Api.DTOs.Songs.SongDto> UploadTestSongAsync(HttpClient adminClient, string title = "Test Song")
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(title), "Title");
            content.Add(new StringContent("Test Artist"), "Artist");

            var fileBytes = "fake mp3 bytes"u8.ToArray();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
            content.Add(fileContent, "File", "test.mp3");

            var response = await adminClient.PostAsync("/api/songs", content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<Api.DTOs.Songs.SongDto>>();
            return body!.Data!;
        }
    }
}
