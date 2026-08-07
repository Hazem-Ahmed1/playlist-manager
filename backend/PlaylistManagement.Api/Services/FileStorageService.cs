using PlaylistManagement.Api.Interfaces;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="IFileStorageService" />
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(string FileName, string RelativePath, long FileSize)> SaveFileAsync(IFormFile file, string subfolder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";

            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"uploads/{subfolder}/{fileName}";

            return (fileName, relativePath, file.Length);
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath))
            {
                return;
            }

            try
            {
                File.Delete(fullPath);
            }
            catch (IOException ex)
            {
                // Keep the DB operation succeeding even if the file is
                // locked/missing — an orphaned file on disk is recoverable,
                // a stuck request is not.
                _logger.LogWarning(ex, "Failed to delete file at {Path}", fullPath);
            }
        }
    }
}
