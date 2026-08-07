namespace PlaylistManagement.Api.Models
{
    /// <summary>
    /// Join entity linking a Playlist to a Song, with a composite primary
    /// key of (PlaylistId, SongId) so a song can only appear once per
    /// playlist. Carries the extra data a plain many-to-many can't: when the
    /// song was added and its position in the playlist.
    /// </summary>
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }

        public Playlist Playlist { get; set; } = null!;

        public int SongId { get; set; }

        public Song Song { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public int Order { get; set; }
    }
}
