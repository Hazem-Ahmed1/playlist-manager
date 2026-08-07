using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlaylistManagement.IntegrationTests.Infrastructure;
using Xunit;

namespace PlaylistManagement.IntegrationTests
{
    /// <summary>
    /// JWT-specific security tests, independent of any one controller:
    /// malformed tokens, expired tokens, and missing tokens must all be
    /// rejected the same way (401), regardless of which protected endpoint
    /// is called.
    /// </summary>
    public class SecurityTests : IClassFixture<PlaylistApiFactory>
    {
        private readonly PlaylistApiFactory _factory;

        public SecurityTests(PlaylistApiFactory factory)
        {
            _factory = factory;
        }

        // Missing JWT returns 401.
        [Fact]
        public async Task GetPlaylists_NoAuthorizationHeader_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/playlists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Invalid JWT returns 401.
        [Fact]
        public async Task GetPlaylists_MalformedToken_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AuthenticateAs("this-is-not-a-real-jwt");

            // Act
            var response = await client.GetAsync("/api/playlists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Invalid JWT (well-formed but wrong signature) returns 401.
        [Fact]
        public async Task GetPlaylists_TokenSignedWithWrongKey_Returns401Unauthorized()
        {
            // Arrange: a syntactically valid, unexpired JWT, but signed with
            // a key the API never issued it with — the signature check must
            // fail even though everything else about the token looks right.
            var client = _factory.CreateClient();
            var bogusToken = JwtTestHelper.CreateTokenWithWrongSigningKey(_factory);
            client.AuthenticateAs(bogusToken);

            // Act
            var response = await client.GetAsync("/api/playlists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Expired JWT returns 401.
        [Fact]
        public async Task GetPlaylists_ExpiredToken_Returns401Unauthorized()
        {
            // Arrange: a validly-signed token whose exp claim is in the
            // past — signature checks out, lifetime validation must reject it.
            var client = _factory.CreateClient();
            var expiredToken = JwtTestHelper.CreateExpiredToken(_factory);
            client.AuthenticateAs(expiredToken);

            // Act
            var response = await client.GetAsync("/api/playlists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // A valid token for a *nonexistent* playlist owner still gets a
        // clean 404, not a 500 — the pipeline handles an unknown but
        // well-formed identity gracefully.
        [Fact]
        public async Task GetPlaylistById_ValidTokenPlaylistNotFound_Returns404NotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await client.RegisterAndGetTokenAsync();
            client.AuthenticateAs(token);

            // Act
            var response = await client.GetAsync("/api/playlists/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
