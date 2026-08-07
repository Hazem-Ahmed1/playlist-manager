using PlaylistManagement.Api.DTOs.Songs;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>Business logic for browsing the song catalog.</summary>
    public interface ISongService
    {
        /// <summary>Gets every song in the catalog.</summary>
        Task<IReadOnlyList<SongDto>> GetAllSongsAsync();

        /// <summary>Gets a single song by id. Throws NotFoundException if it doesn't exist.</summary>
        Task<SongDto> GetSongByIdAsync(int id);

        /// <summary>Saves the uploaded audio file and adds it to the catalog. Admin-only — enforced at the controller, not here.</summary>
        Task<SongDto> UploadSongAsync(UploadSongDto dto);
    }
}
