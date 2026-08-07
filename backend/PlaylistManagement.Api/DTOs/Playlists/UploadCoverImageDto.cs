using System.ComponentModel.DataAnnotations;
using PlaylistManagement.Api.Validation;

namespace PlaylistManagement.Api.DTOs.Playlists
{
    /// <summary>Payload for setting/replacing a playlist's cover image.</summary>
    public class UploadCoverImageDto
    {
        [Required(ErrorMessage = "Cover image file is required.")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" }, ErrorMessage = "Only JPG, PNG, and WEBP images are allowed.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Cover image size cannot exceed 5 MB.")]
        public IFormFile File { get; set; } = null!;
    }
}
