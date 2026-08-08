using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Data access for song metadata. Pure persistence — no business rules.
    /// </summary>
    public interface ISongRepository
    {
        /// <summary>Gets a song by id, or null if it doesn't exist.</summary>
        Task<Song?> GetByIdAsync(int id);

        /// <summary>Gets every song in the catalog, alphabetical by title.</summary>
        Task<IReadOnlyList<Song>> GetAllAsync();

        /// <summary>Tracks a new song for insertion.</summary>
        Task AddAsync(Song song);

        /// <summary>Marks an existing song as modified.</summary>
        void Update(Song song);

        /// <summary>Tracks a song for removal.</summary>
        void Remove(Song song);

        /// <summary>Persists all tracked changes to the database.</summary>
        Task SaveChangesAsync();
    }
}
