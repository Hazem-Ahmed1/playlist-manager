using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Data
{
    /// <summary>
    /// EF Core database context. Extends IdentityDbContext so the standard
    /// Identity tables (AspNetUsers, AspNetRoles, ...) are created alongside
    /// the app's own tables. Entity-specific Fluent API configuration lives
    /// in Data/Configurations and is applied by assembly scan below, rather
    /// than piling everything into this class.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Playlist> Playlists => Set<Playlist>();

        public DbSet<Song> Songs => Set<Song>();

        public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
