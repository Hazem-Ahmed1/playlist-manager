namespace PlaylistManagement.Api.DTOs.Playlists
{
    /// <summary>Full view of a playlist, including its ordered songs.</summary>
    public class PlaylistDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<PlaylistSongDto> Songs { get; set; } = new();
    }

    /// <summary>A song's entry within a specific playlist.</summary>
    public class PlaylistSongDto
    {
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public string? Album { get; set; }

        public TimeSpan Duration { get; set; }

        public int Order { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
