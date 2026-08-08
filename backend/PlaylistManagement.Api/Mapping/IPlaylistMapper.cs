using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Mapping
{
    /// <summary>Entity↔DTO mapping for Playlist, kept out of PlaylistService so the service stays focused on business logic (SRP).</summary>
    public interface IPlaylistMapper
    {
        /// <summary>Maps to the summary DTO used by list endpoints.</summary>
        PlaylistDto ToDto(Playlist playlist);

        /// <summary>Maps to the full DTO, including its ordered songs. Requires PlaylistSongs (and each Song) to be loaded.</summary>
        PlaylistDetailDto ToDetailDto(Playlist playlist);
    }
}
