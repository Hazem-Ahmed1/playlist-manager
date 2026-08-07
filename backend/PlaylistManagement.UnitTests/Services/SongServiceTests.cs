using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Middleware.Exceptions;
using PlaylistManagement.Api.Models;
using PlaylistManagement.Api.Services;
using Xunit;

namespace PlaylistManagement.UnitTests.Services
{
    /// <summary>
    /// SongService unit tests. Dependencies (ISongRepository,
    /// IFileStorageService) are mocked — no database, no file system access.
    ///
    /// Note on file-type/size validation ("Upload invalid file type" /
    /// "Upload file larger than allowed limit" from the spec):
    /// SongService.UploadSongAsync never runs that check itself — it's
    /// enforced by [AllowedExtensions]/[MaxFileSize] Data Annotations on
    /// UploadSongDto.File, validated by ASP.NET Core's model binding before
    /// the controller (and therefore the service) is ever invoked. Testing
    /// those rules against the service would be testing something the
    /// service doesn't own; they're covered as their own unit tests in
    /// FileValidationAttributeTests, and end-to-end via integration tests
    /// asserting a 400 response.
    /// </summary>
    public class SongServiceTests
    {
        private readonly Mock<ISongRepository> _songRepository = new();
        private readonly Mock<IFileStorageService> _fileStorageService = new();
        private readonly SongService _sut;

        public SongServiceTests()
        {
            _sut = new SongService(_songRepository.Object, _fileStorageService.Object);
        }

        // 1. Upload song successfully.
        [Fact]
        public async Task UploadSongAsync_ValidFile_SavesFileAndReturnsSongDto()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("track.mp3");
            file.Setup(f => f.ContentType).Returns("audio/mpeg");
            file.Setup(f => f.Length).Returns(1024);

            var dto = new UploadSongDto
            {
                Title = "New Track",
                Artist = "New Artist",
                File = file.Object
            };

            _fileStorageService
                .Setup(fs => fs.SaveFileAsync(file.Object, "songs"))
                .ReturnsAsync(("guid.mp3", "uploads/songs/guid.mp3", 1024L));

            // Act
            var result = await _sut.UploadSongAsync(dto);

            // Assert: file was persisted to disk, then the resulting path
            // recorded as metadata and handed to the repository.
            result.Title.Should().Be("New Track");
            result.FilePath.Should().Be("uploads/songs/guid.mp3");
            result.FileSize.Should().Be(1024);

            _fileStorageService.Verify(fs => fs.SaveFileAsync(file.Object, "songs"), Times.Once);
            _songRepository.Verify(r => r.AddAsync(It.Is<Song>(s => s.FilePath == "uploads/songs/guid.mp3")), Times.Once);
            _songRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // 5. Delete song successfully.
        [Fact]
        public async Task DeleteSongAsync_ExistingSong_RemovesSongAndDeletesFile()
        {
            // Arrange
            var song = new Song { Id = 10, Title = "Old Track", Artist = "Artist", FilePath = "uploads/songs/old.mp3" };
            _songRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(song);

            // Act
            await _sut.DeleteSongAsync(10);

            // Assert: DB removal and physical file cleanup both happen.
            _fileStorageService.Verify(fs => fs.DeleteFile("uploads/songs/old.mp3"), Times.Once);
            _songRepository.Verify(r => r.Remove(song), Times.Once);
            _songRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // Negative case: deleting a song that doesn't exist.
        [Fact]
        public async Task DeleteSongAsync_SongDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _songRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Song?)null);

            // Act
            var act = async () => await _sut.DeleteSongAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _songRepository.Verify(r => r.Remove(It.IsAny<Song>()), Times.Never);
            _fileStorageService.Verify(fs => fs.DeleteFile(It.IsAny<string>()), Times.Never);
        }

        // Negative case: fetching a song that doesn't exist.
        [Fact]
        public async Task GetSongByIdAsync_SongDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _songRepository.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((Song?)null);

            // Act
            var act = async () => await _sut.GetSongByIdAsync(123);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        // Positive case rounding out the catalog-listing path.
        [Fact]
        public async Task GetAllSongsAsync_CatalogHasSongs_ReturnsAllMappedToDto()
        {
            // Arrange
            var songs = new List<Song>
            {
                new() { Id = 1, Title = "A", Artist = "Artist A" },
                new() { Id = 2, Title = "B", Artist = "Artist B" }
            };
            _songRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(songs);

            // Act
            var result = await _sut.GetAllSongsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(s => s.Title).Should().BeEquivalentTo("A", "B");
        }
    }
}
