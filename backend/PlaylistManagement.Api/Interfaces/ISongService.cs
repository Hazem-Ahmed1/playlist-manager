using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Songs;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Business logic for the song catalog. Reports expected failures (song
    /// not found) via Result instead of throwing.
    /// </summary>
    public interface ISongService
    {
        /// <summary>Gets every song in the catalog.</summary>
        Task<IReadOnlyList<SongDto>> GetAllSongsAsync();

        /// <summary>Gets a single song by id. Fails with NotFound if it doesn't exist.</summary>
        Task<Result<SongDto>> GetSongByIdAsync(int id);

        /// <summary>Saves the uploaded audio file and adds it to the catalog. Admin-only — enforced at the controller, not here.</summary>
        Task<SongDto> UploadSongAsync(UploadSongDto dto);

        /// <summary>
        /// Updates a song's metadata (title/artist/album/genre/duration).
        /// Does not touch the audio file itself. Fails with NotFound if it
        /// doesn't exist. Admin-only — enforced at the controller, not here.
        /// </summary>
        Task<Result<SongDto>> UpdateSongAsync(int id, UpdateSongDto dto);

        /// <summary>
        /// Removes a song from the catalog and deletes its audio file from
        /// disk. The database cascades the removal to any PlaylistSong rows
        /// referencing it. Fails with NotFound if it doesn't exist.
        /// Admin-only — enforced at the controller, not here.
        /// </summary>
        Task<Result> DeleteSongAsync(int id);
    }
}
