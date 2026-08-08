using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.DTOs.Songs
{
    /// <summary>Payload for editing an existing song's metadata. Admin-only — see SongsController. Does not replace the audio file itself.</summary>
    public class UpdateSongDto
    {
        [Required(ErrorMessage = "Song title is required.")]
        [MaxLength(200, ErrorMessage = "Song title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist name is required.")]
        [MaxLength(150, ErrorMessage = "Artist name cannot exceed 150 characters.")]
        public string Artist { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Album cannot exceed 150 characters.")]
        public string? Album { get; set; }

        [MaxLength(100, ErrorMessage = "Genre cannot exceed 100 characters.")]
        public string? Genre { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}
