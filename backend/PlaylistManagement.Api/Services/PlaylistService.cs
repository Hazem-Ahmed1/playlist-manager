using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Middleware.Exceptions;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="IPlaylistService" />
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISongRepository _songRepository;
        private readonly IFileStorageService _fileStorageService;

        public PlaylistService(
            IPlaylistRepository playlistRepository,
            ISongRepository songRepository,
            IFileStorageService fileStorageService)
        {
            _playlistRepository = playlistRepository;
            _songRepository = songRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<PlaylistDto> CreatePlaylistAsync(string userId, CreatePlaylistDto dto)
        {
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

            return MapToDto(playlist);
        }

        public async Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(string userId)
        {
            var playlists = await _playlistRepository.GetByUserIdAsync(userId);
            return playlists.Select(MapToDto).ToList();
        }

        public async Task<PlaylistDetailDto> GetPlaylistByIdAsync(string userId, int playlistId)
        {
            var playlist = await GetOwnedPlaylistWithSongsAsync(userId, playlistId);
            return MapToDetailDto(playlist);
        }

        public async Task DeletePlaylistAsync(string userId, int playlistId)
        {
            var playlist = await GetOwnedPlaylistAsync(userId, playlistId);

            _fileStorageService.DeleteFile(playlist.CoverImagePath);

            _playlistRepository.Remove(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        public async Task<PlaylistDto> UpdatePlaylistAsync(string userId, int playlistId, UpdatePlaylistDto dto)
        {
            var playlist = await GetOwnedPlaylistAsync(userId, playlistId);

            playlist.Name = dto.Name;
            playlist.Description = dto.Description;
            playlist.UpdatedAt = DateTime.UtcNow;

            _playlistRepository.Update(playlist);
            await _playlistRepository.SaveChangesAsync();

            return MapToDto(playlist);
        }

        public async Task<PlaylistDto> UploadCoverImageAsync(string userId, int playlistId, UploadCoverImageDto dto)
        {
            var playlist = await GetOwnedPlaylistAsync(userId, playlistId);

            // Replacing an existing cover shouldn't leave the old file behind.
            _fileStorageService.DeleteFile(playlist.CoverImagePath);

            var (_, relativePath, _) = await _fileStorageService.SaveFileAsync(dto.File, "coverPath");

            playlist.CoverImagePath = relativePath;
            playlist.UpdatedAt = DateTime.UtcNow;

            _playlistRepository.Update(playlist);
            await _playlistRepository.SaveChangesAsync();

            return MapToDto(playlist);
        }

        public async Task AddSongToPlaylistAsync(string userId, int playlistId, AddSongToPlaylistDto dto)
        {
            var playlist = await GetOwnedPlaylistAsync(userId, playlistId);

            var song = await _songRepository.GetByIdAsync(dto.SongId)
                ?? throw new NotFoundException($"Song with id {dto.SongId} was not found.");

            var existingAssociation = await _playlistRepository.GetPlaylistSongAsync(playlistId, song.Id);
            if (existingAssociation is not null)
            {
                throw new BadRequestException("This song is already in the playlist.");
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
        }

        public async Task RemoveSongFromPlaylistAsync(string userId, int playlistId, int songId)
        {
            var playlist = await GetOwnedPlaylistAsync(userId, playlistId);

            var association = await _playlistRepository.GetPlaylistSongAsync(playlistId, songId)
                ?? throw new NotFoundException($"Song with id {songId} was not found in this playlist.");

            _playlistRepository.RemoveSong(association);

            playlist.UpdatedAt = DateTime.UtcNow;
            _playlistRepository.Update(playlist);

            await _playlistRepository.SaveChangesAsync();
        }

        /// <summary>Loads a playlist (with its songs, for SongCount) and enforces that it belongs to the given user.</summary>
        private async Task<Playlist> GetOwnedPlaylistAsync(string userId, int playlistId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId)
                ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");

            EnsureOwnership(userId, playlist);

            return playlist;
        }

        private async Task<Playlist> GetOwnedPlaylistWithSongsAsync(string userId, int playlistId)
        {
            var playlist = await _playlistRepository.GetByIdWithSongsAsync(playlistId)
                ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");

            EnsureOwnership(userId, playlist);

            return playlist;
        }

        private static void EnsureOwnership(string userId, Playlist playlist)
        {
            if (playlist.UserId != userId)
            {
                throw new ForbiddenAccessException("You do not have access to this playlist.");
            }
        }

        private static PlaylistDto MapToDto(Playlist playlist) => new()
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            CoverImagePath = playlist.CoverImagePath,
            SongCount = playlist.PlaylistSongs.Count,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt
        };

        private static PlaylistDetailDto MapToDetailDto(Playlist playlist) => new()
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
                    Order = ps.Order,
                    AddedAt = ps.AddedAt
                })
                .ToList()
        };
    }
}
