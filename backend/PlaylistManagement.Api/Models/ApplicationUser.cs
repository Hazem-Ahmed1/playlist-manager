using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PlaylistManagement.Api.Models
{
    /// <summary>
    /// Application user. Extends the built-in Identity user with the profile
    /// fields this app needs. Email/password format rules are enforced on the
    /// registration DTO, not here — Identity never stores a raw password on
    /// this entity, only its hash.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Playlists owned by this user. Deleting the user cascades to these.
        /// </summary>
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
