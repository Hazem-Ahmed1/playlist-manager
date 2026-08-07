using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlaylistManagement.Api.Models.Options;

namespace PlaylistManagement.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Mints a validly-signed but already-expired JWT for the "expired
    /// token" security test. Reads the real Key/Issuer/Audience from the
    /// running test host via DI instead of duplicating the values, so this
    /// can't silently drift from appsettings and produce a false pass/fail
    /// for the wrong reason (bad signature vs. expired lifetime).
    /// </summary>
    public static class JwtTestHelper
    {
        public static string CreateExpiredToken(PlaylistApiFactory factory, string userId = "expired-user", string email = "expired@example.com")
        {
            using var scope = factory.Services.CreateScope();
            var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-10),
                expires: DateTime.UtcNow.AddMinutes(-5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// A well-formed, unexpired JWT signed with a key the API never
        /// issued it with — isolates signature validation from lifetime
        /// validation (see CreateExpiredToken).
        /// </summary>
        public static string CreateTokenWithWrongSigningKey(PlaylistApiFactory factory, string userId = "intruder", string email = "intruder@example.com")
        {
            using var scope = factory.Services.CreateScope();
            var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-key-does-not-match-the-apis-configured-secret"));
            var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
