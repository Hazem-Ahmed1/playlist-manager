using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.Models
{
    /// <summary>
    /// Metadata for an uploaded audio file. The actual file lives on disk
    /// under wwwroot/uploads/songs with a GUID-based file name; this entity
    /// never stores file bytes, only the metadata describing them.
    /// </summary>
    public class Song
    {
        public int Id { get; set; }

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

        public TimeSpan Duration { get; set; }

        [Required]
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string FileExtension { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
