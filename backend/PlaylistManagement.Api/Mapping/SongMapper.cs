using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Mapping
{
    /// <inheritdoc cref="ISongMapper" />
    public class SongMapper : ISongMapper
    {
        public SongDto ToDto(Song song) => new()
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            Album = song.Album,
            Genre = song.Genre,
            Duration = song.Duration,
            FilePath = song.FilePath,
            FileSize = song.FileSize,
            UploadedAt = song.UploadedAt
        };
    }
}
