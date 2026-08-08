using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Mapping
{
    /// <inheritdoc cref="IPlaylistMapper" />
    public class PlaylistMapper : IPlaylistMapper
    {
        public PlaylistDto ToDto(Playlist playlist) => new()
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            CoverImagePath = playlist.CoverImagePath,
            SongCount = playlist.PlaylistSongs.Count,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt
        };

        public PlaylistDetailDto ToDetailDto(Playlist playlist) => new()
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            CoverImagePath = playlist.CoverImagePath,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt,
            Songs = playlist.PlaylistSongs
                .OrderBy(ps => ps.Order)
                .Select(ps => new PlaylistSongDto
                {
                    SongId = ps.SongId,
                    Title = ps.Song.Title,
                    Artist = ps.Song.Artist,
                    Album = ps.Song.Album,
                    Duration = ps.Song.Duration,
                    FilePath = ps.Song.FilePath,
                    Order = ps.Order,
                    AddedAt = ps.AddedAt
                })
                .ToList()
        };
    }
}
