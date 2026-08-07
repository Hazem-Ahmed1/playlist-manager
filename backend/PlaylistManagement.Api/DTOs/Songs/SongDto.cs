namespace PlaylistManagement.Api.DTOs.Songs
{
    /// <summary>Catalog view of a song, used when browsing available songs to add to a playlist.</summary>
    public class SongDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public string? Album { get; set; }

        public string? Genre { get; set; }

        public TimeSpan Duration { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
