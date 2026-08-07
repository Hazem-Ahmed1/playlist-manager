using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PlaylistManagement.Api;
using PlaylistManagement.Api.Data;
using Xunit;

namespace PlaylistManagement.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Boots the real API in-process against a SQLite in-memory database
    /// instead of the configured SQL Server instance, so integration tests
    /// exercise the actual HTTP pipeline (routing, model validation, auth,
    /// exception middleware, EF Core) without needing a real SQL Server.
    ///
    /// SQLite's in-memory mode only persists for as long as a connection to
    /// it stays open, so a single SqliteConnection is opened here and kept
    /// alive for the factory's lifetime, and shared by every DbContext the
    /// app creates. Schema is built with EnsureCreated (not the app's real
    /// migrations, which contain SQL Server-specific syntax SQLite can't
    /// run) before the host starts, so Program.cs's own startup seeding
    /// (DataSeeder) has tables to write to.
    /// </summary>
    public class PlaylistApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            await using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // The app's default Development logging is very chatty (every
            // EF Core SQL statement) and drowns out test output for no
            // benefit here.
            builder.ConfigureLogging(logging =>
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            });

            builder.ConfigureServices(services =>
            {
                // AddDbContext doesn't simply overwrite a prior registration
                // for the same TContext — it chains configuration via
                // IDbContextOptionsConfiguration<T>. Removing only
                // DbContextOptions<T> leaves the app's UseSqlServer callback
                // registered too, so both providers end up applied to the
                // same options builder. Strip every EF registration for
                // ApplicationDbContext before adding the SQLite one.
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            });
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            _connection.Dispose();
            return Task.CompletedTask;
        }
    }
}
