using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Mapping
{
    /// <summary>Entity↔DTO mapping for Song, kept out of SongService so the service stays focused on business logic (SRP).</summary>
    public interface ISongMapper
    {
        SongDto ToDto(Song song);
    }
}
