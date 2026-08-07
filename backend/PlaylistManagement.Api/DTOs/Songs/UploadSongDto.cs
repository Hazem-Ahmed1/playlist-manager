using System.ComponentModel.DataAnnotations;
using PlaylistManagement.Api.Validation;

namespace PlaylistManagement.Api.DTOs.Songs
{
    /// <summary>Payload for adding a new song to the catalog. Admin-only — see SongsController.</summary>
    public class UploadSongDto
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

        /// <summary>Track length. Optional — audio duration isn't parsed server-side, so provide it manually (e.g. "00:03:30").</summary>
        public TimeSpan? Duration { get; set; }

        [Required(ErrorMessage = "Song file is required.")]
        [AllowedExtensions(new[] { ".mp3", ".wav", ".m4a" }, ErrorMessage = "Only MP3, WAV, and M4A files are allowed.")]
        [MaxFileSize(20 * 1024 * 1024, ErrorMessage = "File size cannot exceed 20 MB.")]
        public IFormFile File { get; set; } = null!;
    }
}
