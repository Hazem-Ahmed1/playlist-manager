using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Mapping;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="ISongService" />
    public class SongService : ISongService
    {
        private readonly ISongRepository _songRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISongMapper _mapper;

        public SongService(ISongRepository songRepository, IFileStorageService fileStorageService, ISongMapper mapper)
        {
            _songRepository = songRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<SongDto>> GetAllSongsAsync()
        {
            var songs = await _songRepository.GetAllAsync();
            return songs.Select(_mapper.ToDto).ToList();
        }

        public async Task<Result<SongDto>> GetSongByIdAsync(int id)
        {
            var song = await _songRepository.GetByIdAsync(id);
            if (song is null)
            {
                return Result<SongDto>.Failure(ErrorType.NotFound, $"Song with id {id} was not found.");
            }

            return Result<SongDto>.Success(_mapper.ToDto(song));
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

            return _mapper.ToDto(song);
        }

        public async Task<Result<SongDto>> UpdateSongAsync(int id, UpdateSongDto dto)
        {
            var song = await _songRepository.GetByIdAsync(id);
            if (song is null)
            {
                return Result<SongDto>.Failure(ErrorType.NotFound, $"Song with id {id} was not found.");
            }

            song.Title = dto.Title;
            song.Artist = dto.Artist;
            song.Album = dto.Album;
            song.Genre = dto.Genre;
            song.Duration = dto.Duration ?? song.Duration;

            _songRepository.Update(song);
            await _songRepository.SaveChangesAsync();

            return Result<SongDto>.Success(_mapper.ToDto(song));
        }

        public async Task<Result> DeleteSongAsync(int id)
        {
            var song = await _songRepository.GetByIdAsync(id);
            if (song is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Song with id {id} was not found.");
            }

            _fileStorageService.DeleteFile(song.FilePath);

            _songRepository.Remove(song);
            await _songRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}
