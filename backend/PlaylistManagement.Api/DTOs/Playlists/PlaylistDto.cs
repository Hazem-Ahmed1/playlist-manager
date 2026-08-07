namespace PlaylistManagement.Api.DTOs.Playlists
{
    /// <summary>Summary view of a playlist, used for list endpoints.</summary>
    public class PlaylistDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverImagePath { get; set; }

        public int SongCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
