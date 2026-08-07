using Microsoft.EntityFrameworkCore;
using PlaylistManagement.Api.Data;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Repositories
{
    /// <inheritdoc cref="IPlaylistRepository" />
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ApplicationDbContext _context;

        public PlaylistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Includes PlaylistSongs (no further ThenInclude) so SongCount is
        // accurate wherever this feeds a PlaylistDto — GetByIdWithSongsAsync
        // below is the one that also loads each Song for full detail views.
        public Task<Playlist?> GetByIdAsync(int id) =>
            _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == id);

        public Task<Playlist?> GetByIdWithSongsAsync(int id) =>
            _context.Playlists
                .Include(p => p.PlaylistSongs.OrderBy(ps => ps.Order))
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyList<Playlist>> GetByUserIdAsync(string userId) =>
            await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task AddAsync(Playlist playlist) =>
            await _context.Playlists.AddAsync(playlist);

        public void Update(Playlist playlist) =>
            _context.Playlists.Update(playlist);

        public void Remove(Playlist playlist) =>
            _context.Playlists.Remove(playlist);

        public Task<PlaylistSong?> GetPlaylistSongAsync(int playlistId, int songId) =>
            _context.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

        public async Task<int> GetNextSongOrderAsync(int playlistId)
        {
            var maxOrder = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Select(ps => (int?)ps.Order)
                .MaxAsync();

            return (maxOrder ?? 0) + 1;
        }

        public async Task AddSongAsync(PlaylistSong playlistSong) =>
            await _context.PlaylistSongs.AddAsync(playlistSong);

        public void RemoveSong(PlaylistSong playlistSong) =>
            _context.PlaylistSongs.Remove(playlistSong);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
