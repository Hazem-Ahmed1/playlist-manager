using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Data access for playlists and their song associations. Pure
    /// persistence — no ownership checks, no business rules. Those live in
    /// IPlaylistService.
    /// </summary>
    public interface IPlaylistRepository
    {
        /// <summary>Gets a playlist by id, with no related data loaded.</summary>
        Task<Playlist?> GetByIdAsync(int id);

        /// <summary>Gets a playlist by id including its songs (via PlaylistSongs), ordered by track order.</summary>
        Task<Playlist?> GetByIdWithSongsAsync(int id);

        /// <summary>Gets every playlist owned by the given user, most recently created first.</summary>
        Task<IReadOnlyList<Playlist>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Checks whether the user already has a playlist with this name
        /// (case-insensitive). Pass excludePlaylistId when checking during
        /// a rename, so a playlist doesn't collide with its own name.
        /// </summary>
        Task<bool> ExistsByNameAsync(string userId, string name, int? excludePlaylistId = null);

        /// <summary>Tracks a new playlist for insertion.</summary>
        Task AddAsync(Playlist playlist);

        /// <summary>Marks an existing playlist as modified.</summary>
        void Update(Playlist playlist);

        /// <summary>Tracks a playlist for removal.</summary>
        void Remove(Playlist playlist);

        /// <summary>Gets the join row for a specific playlist/song pair, or null if the song isn't in the playlist.</summary>
        Task<PlaylistSong?> GetPlaylistSongAsync(int playlistId, int songId);

        /// <summary>Computes the next track order value for a playlist (current max + 1).</summary>
        Task<int> GetNextSongOrderAsync(int playlistId);

        /// <summary>Tracks a new playlist/song association for insertion.</summary>
        Task AddSongAsync(PlaylistSong playlistSong);

        /// <summary>Tracks a playlist/song association for removal.</summary>
        void RemoveSong(PlaylistSong playlistSong);

        /// <summary>Persists all tracked changes to the database.</summary>
        Task SaveChangesAsync();
    }
}
