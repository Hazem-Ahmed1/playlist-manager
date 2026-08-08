using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Playlists;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Business logic for playlists: ownership enforcement, validation
    /// beyond Data Annotations, and entity/DTO mapping. Reports expected
    /// failures (not found, forbidden, duplicate name) via Result instead of
    /// throwing.
    /// </summary>
    public interface IPlaylistService
    {
        /// <summary>Creates a new playlist owned by the given user. Fails with BadRequest if the user already has a playlist with this name.</summary>
        Task<Result<PlaylistDto>> CreatePlaylistAsync(string userId, CreatePlaylistDto dto);

        /// <summary>Gets every playlist owned by the given user.</summary>
        Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(string userId);

        /// <summary>
        /// Gets a single playlist with its songs. Fails with NotFound if it
        /// doesn't exist, Forbidden if it belongs to another user.
        /// </summary>
        Task<Result<PlaylistDetailDto>> GetPlaylistByIdAsync(string userId, int playlistId);

        /// <summary>
        /// Deletes a playlist owned by the given user, including its cover
        /// image file on disk. Fails under the same conditions as
        /// GetPlaylistByIdAsync.
        /// </summary>
        Task<Result> DeletePlaylistAsync(string userId, int playlistId);

        /// <summary>
        /// Updates a playlist's name/description. Fails with NotFound/Forbidden
        /// under the same conditions as GetPlaylistByIdAsync, or BadRequest if
        /// the new name collides with another of the user's playlists.
        /// </summary>
        Task<Result<PlaylistDto>> UpdatePlaylistAsync(string userId, int playlistId, UpdatePlaylistDto dto);

        /// <summary>
        /// Sets or replaces a playlist's cover image, deleting the previous
        /// file if one existed. Fails under the same conditions as
        /// GetPlaylistByIdAsync.
        /// </summary>
        Task<Result<PlaylistDto>> UploadCoverImageAsync(string userId, int playlistId, UploadCoverImageDto dto);

        /// <summary>
        /// Adds an existing song to a playlist owned by the given user.
        /// Fails with NotFound if the playlist or song doesn't exist,
        /// Forbidden if the playlist belongs to another user, BadRequest if
        /// the song is already in the playlist.
        /// </summary>
        Task<Result> AddSongToPlaylistAsync(string userId, int playlistId, AddSongToPlaylistDto dto);

        /// <summary>
        /// Removes a song from a playlist owned by the given user. Fails
        /// with NotFound if the playlist or association doesn't exist,
        /// Forbidden if the playlist belongs to another user.
        /// </summary>
        Task<Result> RemoveSongFromPlaylistAsync(string userId, int playlistId, int songId);
    }
}
