using System.ComponentModel.DataAnnotations;

namespace PlaylistManagement.Api.DTOs.Playlists
{
    /// <summary>Payload for adding an existing song to a playlist.</summary>
    public class AddSongToPlaylistDto
    {
        [Required(ErrorMessage = "Song id is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Song id must be a positive number.")]
        public int SongId { get; set; }
    }
}
