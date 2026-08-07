using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.DTOs.Playlists
{
    /// <summary>Payload for updating a playlist's name/description.</summary>
    public class UpdatePlaylistDto
    {
        [Required(ErrorMessage = "Playlist name is required.")]
        [MaxLength(100, ErrorMessage = "Playlist name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
