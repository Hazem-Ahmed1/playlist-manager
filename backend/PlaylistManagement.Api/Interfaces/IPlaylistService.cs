using PlaylistManagement.Api.DTOs.Playlists;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Business logic for playlists: ownership enforcement, validation
    /// beyond Data Annotations, and entity/DTO mapping.
    /// </summary>
    public interface IPlaylistService
    {
        /// <summary>Creates a new playlist owned by the given user.</summary>
        Task<PlaylistDto> CreatePlaylistAsync(string userId, CreatePlaylistDto dto);

        /// <summary>Gets every playlist owned by the given user.</summary>
        Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(string userId);

        /// <summary>
        /// Gets a single playlist with its songs. Throws NotFoundException if
        /// it doesn't exist, ForbiddenAccessException if it belongs to
        /// another user.
        /// </summary>
        Task<PlaylistDetailDto> GetPlaylistByIdAsync(string userId, int playlistId);

        /// <summary>
        /// Deletes a playlist owned by the given user, including its cover
        /// image file on disk. Throws NotFoundException/ForbiddenAccessException
        /// under the same conditions as GetPlaylistByIdAsync.
        /// </summary>
        Task DeletePlaylistAsync(string userId, int playlistId);

        /// <summary>
        /// Updates a playlist's name/description. Throws
        /// NotFoundException/ForbiddenAccessException under the same
        /// conditions as GetPlaylistByIdAsync.
        /// </summary>
        Task<PlaylistDto> UpdatePlaylistAsync(string userId, int playlistId, UpdatePlaylistDto dto);

        /// <summary>
        /// Sets or replaces a playlist's cover image, deleting the previous
        /// file if one existed. Throws NotFoundException/ForbiddenAccessException
        /// under the same conditions as GetPlaylistByIdAsync.
        /// </summary>
        Task<PlaylistDto> UploadCoverImageAsync(string userId, int playlistId, UploadCoverImageDto dto);

        /// <summary>
        /// Adds an existing song to a playlist owned by the given user.
        /// Throws NotFoundException if the playlist or song doesn't exist,
        /// ForbiddenAccessException if the playlist belongs to another user,
        /// BadRequestException if the song is already in the playlist.
        /// </summary>
        Task AddSongToPlaylistAsync(string userId, int playlistId, AddSongToPlaylistDto dto);

        /// <summary>
        /// Removes a song from a playlist owned by the given user. Throws
        /// NotFoundException if the playlist or association doesn't exist,
        /// ForbiddenAccessException if the playlist belongs to another user.
        /// </summary>
        Task RemoveSongFromPlaylistAsync(string userId, int playlistId, int songId);
    }
}
