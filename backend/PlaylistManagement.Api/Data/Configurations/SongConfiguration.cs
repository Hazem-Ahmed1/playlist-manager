using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Data.Configurations
{
    public class SongConfiguration : IEntityTypeConfiguration<Song>
    {
        public void Configure(EntityTypeBuilder<Song> builder)
        {
            builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
            builder.Property(s => s.Artist).HasMaxLength(150).IsRequired();
            builder.Property(s => s.Album).HasMaxLength(150);
            builder.Property(s => s.Genre).HasMaxLength(100);
            builder.Property(s => s.FilePath).HasMaxLength(500).IsRequired();
            builder.Property(s => s.FileExtension).HasMaxLength(10).IsRequired();
            builder.Property(s => s.ContentType).HasMaxLength(100).IsRequired();

            // Deleting a song only clears its join rows; the physical audio
            // file is removed by SongService, not by the database.
            builder.HasMany(s => s.PlaylistSongs)
                .WithOne(ps => ps.Song)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.Title);
            builder.HasIndex(s => s.Artist);

            builder.HasData(GetSeedSongs());
        }

        /// <summary>
        /// Five placeholder songs so playlists can be exercised end-to-end
        /// without a working upload endpoint. Metadata only — the actual
        /// audio bytes are not part of the repo; drop matching files named
        /// 1.mp3 .. 5.mp3 into wwwroot/uploads/songs to make them playable.
        /// Timestamps are fixed (not DateTime.UtcNow) because EF's HasData
        /// seed values must be deterministic across migrations.
        /// </summary>
        private static IEnumerable<Song> GetSeedSongs() => new[]
        {
            new Song
            {
                Id = 1,
                Title = "Blinding Lights",
                Artist = "The Weeknd",
                Album = "After Hours",
                Genre = "Synth-pop",
                Duration = new TimeSpan(0, 3, 20),
                FileName = "1.mp3",
                FilePath = "uploads/songs/1.mp3",
                FileExtension = ".mp3",
                FileSize = 5_400_000,
                ContentType = "audio/mpeg",
                UploadedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Song
            {
                Id = 2,
                Title = "Shape of You",
                Artist = "Ed Sheeran",
                Album = "÷ (Divide)",
                Genre = "Pop",
                Duration = new TimeSpan(0, 3, 53),
                FileName = "2.mp3",
                FilePath = "uploads/songs/2.mp3",
                FileExtension = ".mp3",
                FileSize = 6_100_000,
                ContentType = "audio/mpeg",
                UploadedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Song
            {
                Id = 3,
                Title = "Bohemian Rhapsody",
                Artist = "Queen",
                Album = "A Night at the Opera",
                Genre = "Rock",
                Duration = new TimeSpan(0, 5, 55),
                FileName = "3.mp3",
                FilePath = "uploads/songs/3.mp3",
                FileExtension = ".mp3",
                FileSize = 8_800_000,
                ContentType = "audio/mpeg",
                UploadedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Song
            {
                Id = 4,
                Title = "Levitating",
                Artist = "Dua Lipa",
                Album = "Future Nostalgia",
                Genre = "Disco-pop",
                Duration = new TimeSpan(0, 3, 23),
                FileName = "4.mp3",
                FilePath = "uploads/songs/4.mp3",
                FileExtension = ".mp3",
                FileSize = 5_600_000,
                ContentType = "audio/mpeg",
                UploadedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Song
            {
                Id = 5,
                Title = "Stairway to Heaven",
                Artist = "Led Zeppelin",
                Album = "Led Zeppelin IV",
                Genre = "Rock",
                Duration = new TimeSpan(0, 8, 2),
                FileName = "5.mp3",
                FilePath = "uploads/songs/5.mp3",
                FileExtension = ".mp3",
                FileSize = 12_500_000,
                ContentType = "audio/mpeg",
                UploadedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }
}
