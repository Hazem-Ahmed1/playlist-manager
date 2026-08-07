using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.Models
{
    /// <summary>
    /// A user-owned collection of songs. The cover image, if any, is stored
    /// on disk under wwwroot/uploads/coverPath with a GUID-based file name;
    /// this only holds the resulting relative path.
    /// </summary>
    public class Playlist
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Playlist name is required.")]
        [MaxLength(100, ErrorMessage = "Playlist name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public string? CoverImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
