namespace PlaylistManagement.Api.Interfaces
{
    /// <summary>
    /// Saves and deletes uploaded files under wwwroot. Kept separate from
    /// SongService/PlaylistService (SRP) so "how a file lands on disk" isn't
    /// duplicated across every entity that has an upload.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the file under wwwroot/uploads/{subfolder} with a GUID-based
        /// name (original extension preserved), creating the folder if
        /// needed. Returns the generated file name, the size in bytes, and
        /// the path to store in the database (relative to wwwroot, forward
        /// slashes, e.g. "uploads/songs/{guid}.mp3").
        /// </summary>
        Task<(string FileName, string RelativePath, long FileSize)> SaveFileAsync(IFormFile file, string subfolder);

        /// <summary>
        /// Deletes the file at the given wwwroot-relative path, if it
        /// exists. No-ops for null/empty input or a missing file.
        /// </summary>
        void DeleteFile(string? relativePath);
    }
}
