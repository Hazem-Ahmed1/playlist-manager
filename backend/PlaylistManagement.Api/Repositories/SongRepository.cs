using Microsoft.EntityFrameworkCore;
using PlaylistManagement.Api.Data;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Repositories
{
    /// <inheritdoc cref="ISongRepository" />
    public class SongRepository : ISongRepository
    {
        private readonly ApplicationDbContext _context;

        public SongRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Song?> GetByIdAsync(int id) =>
            _context.Songs.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IReadOnlyList<Song>> GetAllAsync() =>
            await _context.Songs
                .OrderBy(s => s.Title)
                .ToListAsync();

        public Task<bool> ExistsAsync(int id) =>
            _context.Songs.AnyAsync(s => s.Id == id);

        public async Task AddAsync(Song song) =>
            await _context.Songs.AddAsync(song);

        public void Remove(Song song) =>
            _context.Songs.Remove(song);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
