using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Mapping;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="IPlaylistService" />
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISongRepository _songRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPlaylistMapper _mapper;

        public PlaylistService(
            IPlaylistRepository playlistRepository,
            ISongRepository songRepository,
            IFileStorageService fileStorageService,
            IPlaylistMapper mapper)
        {
            _playlistRepository = playlistRepository;
            _songRepository = songRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<Result<PlaylistDto>> CreatePlaylistAsync(string userId, CreatePlaylistDto dto)
        {
            if (await _playlistRepository.ExistsByNameAsync(userId, dto.Name))
            {
                return Result<PlaylistDto>.Failure(ErrorType.BadRequest, "You already have a playlist with this name.");
            }

            var playlist = new Playlist
            {
                Name = dto.Name,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _playlistRepository.AddAsync(playlist);
            await _playlistRepository.SaveChangesAsync();

            return Result<PlaylistDto>.Success(_mapper.ToDto(playlist));
        }

        public async Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(string userId)
        {
            var playlists = await _playlistRepository.GetByUserIdAsync(userId);
            return playlists.Select(_mapper.ToDto).ToList();
        }

        public async Task<Result<PlaylistDetailDto>> GetPlaylistByIdAsync(string userId, int playlistId)
        {
            var lookup = await GetOwnedPlaylistWithSongsAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result<PlaylistDetailDto>.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            return Result<PlaylistDetailDto>.Success(_mapper.ToDetailDto(lookup.Value!));
        }

        public async Task<Result> DeletePlaylistAsync(string userId, int playlistId)
        {
            var lookup = await GetOwnedPlaylistAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            var playlist = lookup.Value!;
            _fileStorageService.DeleteFile(playlist.CoverImagePath);

            _playlistRepository.Remove(playlist);
            await _playlistRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<PlaylistDto>> UpdatePlaylistAsync(string userId, int playlistId, UpdatePlaylistDto dto)
        {
            var lookup = await GetOwnedPlaylistAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result<PlaylistDto>.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            var playlist = lookup.Value!;

            if (await _playlistRepository.ExistsByNameAsync(userId, dto.Name, excludePlaylistId: playlistId))
            {
                return Result<PlaylistDto>.Failure(ErrorType.BadRequest, "You already have a playlist with this name.");
            }

            playlist.Name = dto.Name;
            playlist.Description = dto.Description;
            playlist.UpdatedAt = DateTime.UtcNow;

            _playlistRepository.Update(playlist);
            await _playlistRepository.SaveChangesAsync();

            return Result<PlaylistDto>.Success(_mapper.ToDto(playlist));
        }

        public async Task<Result<PlaylistDto>> UploadCoverImageAsync(string userId, int playlistId, UploadCoverImageDto dto)
        {
            var lookup = await GetOwnedPlaylistAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result<PlaylistDto>.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            var playlist = lookup.Value!;

            // Replacing an existing cover shouldn't leave the old file behind.
            _fileStorageService.DeleteFile(playlist.CoverImagePath);

            var (_, relativePath, _) = await _fileStorageService.SaveFileAsync(dto.File, "coverPath");

            playlist.CoverImagePath = relativePath;
            playlist.UpdatedAt = DateTime.UtcNow;

            _playlistRepository.Update(playlist);
            await _playlistRepository.SaveChangesAsync();

            return Result<PlaylistDto>.Success(_mapper.ToDto(playlist));
        }

        public async Task<Result> AddSongToPlaylistAsync(string userId, int playlistId, AddSongToPlaylistDto dto)
        {
            var lookup = await GetOwnedPlaylistAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            var playlist = lookup.Value!;

            var song = await _songRepository.GetByIdAsync(dto.SongId);
            if (song is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Song with id {dto.SongId} was not found.");
            }

            var existingAssociation = await _playlistRepository.GetPlaylistSongAsync(playlistId, song.Id);
            if (existingAssociation is not null)
            {
                return Result.Failure(ErrorType.BadRequest, "This song is already in the playlist.");
            }

            var nextOrder = await _playlistRepository.GetNextSongOrderAsync(playlistId);

            await _playlistRepository.AddSongAsync(new PlaylistSong
            {
                PlaylistId = playlist.Id,
                SongId = song.Id,
                Order = nextOrder,
                AddedAt = DateTime.UtcNow
            });

            playlist.UpdatedAt = DateTime.UtcNow;
            _playlistRepository.Update(playlist);

            await _playlistRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> RemoveSongFromPlaylistAsync(string userId, int playlistId, int songId)
        {
            var lookup = await GetOwnedPlaylistAsync(userId, playlistId);
            if (!lookup.IsSuccess)
            {
                return Result.Failure(lookup.ErrorType, lookup.ErrorMessage!);
            }

            var playlist = lookup.Value!;

            var association = await _playlistRepository.GetPlaylistSongAsync(playlistId, songId);
            if (association is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Song with id {songId} was not found in this playlist.");
            }

            _playlistRepository.RemoveSong(association);

            playlist.UpdatedAt = DateTime.UtcNow;
            _playlistRepository.Update(playlist);

            await _playlistRepository.SaveChangesAsync();

            return Result.Success();
        }

        /// <summary>Loads a playlist (with its songs, for SongCount) and enforces that it belongs to the given user.</summary>
        private async Task<Result<Playlist>> GetOwnedPlaylistAsync(string userId, int playlistId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);
            if (playlist is null)
            {
                return Result<Playlist>.Failure(ErrorType.NotFound, $"Playlist with id {playlistId} was not found.");
            }

            return EnsureOwnership(userId, playlist);
        }

        private async Task<Result<Playlist>> GetOwnedPlaylistWithSongsAsync(string userId, int playlistId)
        {
            var playlist = await _playlistRepository.GetByIdWithSongsAsync(playlistId);
            if (playlist is null)
            {
                return Result<Playlist>.Failure(ErrorType.NotFound, $"Playlist with id {playlistId} was not found.");
            }

            return EnsureOwnership(userId, playlist);
        }

        private static Result<Playlist> EnsureOwnership(string userId, Playlist playlist) =>
            playlist.UserId == userId
                ? Result<Playlist>.Success(playlist)
                : Result<Playlist>.Failure(ErrorType.Forbidden, "You do not have access to this playlist.");
    }
}
