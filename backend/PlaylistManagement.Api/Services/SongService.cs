using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Middleware.Exceptions;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="ISongService" />
    public class SongService : ISongService
    {
        private readonly ISongRepository _songRepository;
        private readonly IFileStorageService _fileStorageService;

        public SongService(ISongRepository songRepository, IFileStorageService fileStorageService)
        {
            _songRepository = songRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IReadOnlyList<SongDto>> GetAllSongsAsync()
        {
            var songs = await _songRepository.GetAllAsync();
            return songs.Select(MapToDto).ToList();
        }

        public async Task<SongDto> GetSongByIdAsync(int id)
        {
            var song = await _songRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Song with id {id} was not found.");

            return MapToDto(song);
        }

        public async Task<SongDto> UploadSongAsync(UploadSongDto dto)
        {
            var (fileName, relativePath, fileSize) = await _fileStorageService.SaveFileAsync(dto.File, "songs");

            var song = new Song
            {
                Title = dto.Title,
                Artist = dto.Artist,
                Album = dto.Album,
                Genre = dto.Genre,
                Duration = dto.Duration ?? TimeSpan.Zero,
                FileName = fileName,
                FilePath = relativePath,
                FileExtension = Path.GetExtension(dto.File.FileName).ToLowerInvariant(),
                FileSize = fileSize,
                ContentType = dto.File.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            await _songRepository.AddAsync(song);
            await _songRepository.SaveChangesAsync();

            return MapToDto(song);
        }

        private static SongDto MapToDto(Song song) => new()
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
