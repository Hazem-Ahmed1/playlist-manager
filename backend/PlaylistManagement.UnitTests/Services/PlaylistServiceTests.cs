using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Mapping;
using PlaylistManagement.Api.Models;
using PlaylistManagement.Api.Services;
using Xunit;

namespace PlaylistManagement.UnitTests.Services
{
    /// <summary>
    /// PlaylistService unit tests. All dependencies (IPlaylistRepository,
    /// ISongRepository, IFileStorageService) are mocked — no database, no
    /// file system access. PlaylistMapper is used as a real instance since
    /// it's pure mapping logic, not a collaborator worth mocking.
    /// </summary>
    public class PlaylistServiceTests
    {
        private readonly Mock<IPlaylistRepository> _playlistRepository = new();
        private readonly Mock<ISongRepository> _songRepository = new();
        private readonly Mock<IFileStorageService> _fileStorageService = new();
        private readonly PlaylistService _sut;

        public PlaylistServiceTests()
        {
            _sut = new PlaylistService(_playlistRepository.Object, _songRepository.Object, _fileStorageService.Object, new PlaylistMapper());
        }

        // 1. Create Playlist successfully.
        [Fact]
        public async Task CreatePlaylistAsync_ValidData_ReturnsCreatedPlaylistDto()
        {
            // Arrange
            const string userId = "user-1";
            var dto = new CreatePlaylistDto { Name = "Road Trip", Description = "Driving songs" };

            // Act
            var result = await _sut.CreatePlaylistAsync(userId, dto);

            // Assert: the returned DTO reflects the input, and the new
            // playlist was actually handed to the repository for insertion.
            result.IsSuccess.Should().BeTrue();
            result.Value!.Name.Should().Be(dto.Name);
            result.Value.Description.Should().Be(dto.Description);
            result.Value.SongCount.Should().Be(0);

            _playlistRepository.Verify(r => r.AddAsync(It.Is<Playlist>(p => p.Name == dto.Name && p.UserId == userId)), Times.Once);
            _playlistRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // 2. Creating a playlist with an invalid user.
        // PlaylistService trusts the caller-supplied userId (ownership is
        // established by the JWT, not re-validated here) — so "invalid
        // user" surfaces as a persistence failure (e.g. a foreign key
        // violation against AspNetUsers), which is a genuinely unexpected
        // failure, not a business-rule Result — so it still propagates as
        // an exception rather than being swallowed.
        [Fact]
        public async Task CreatePlaylistAsync_PersistenceFailsForInvalidUser_PropagatesException()
        {
            // Arrange
            const string invalidUserId = "does-not-exist";
            var dto = new CreatePlaylistDto { Name = "Orphan Playlist" };

            _playlistRepository
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateException("FK violation on UserId"));

            // Act
            var act = async () => await _sut.CreatePlaylistAsync(invalidUserId, dto);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();
        }

        // 3. Get all playlists for the current user.
        [Fact]
        public async Task GetUserPlaylistsAsync_UserHasPlaylists_ReturnsOnlyThatUsersPlaylists()
        {
            // Arrange
            const string userId = "user-1";
            var playlists = new List<Playlist>
            {
                new() { Id = 1, Name = "Gym", UserId = userId, PlaylistSongs = new List<PlaylistSong>() },
                new() { Id = 2, Name = "Chill", UserId = userId, PlaylistSongs = new List<PlaylistSong>() }
            };

            _playlistRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(playlists);

            // Act
            var result = await _sut.GetUserPlaylistsAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().BeEquivalentTo("Gym", "Chill");
            _playlistRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }

        // 4. User cannot access another user's playlist.
        [Fact]
        public async Task GetPlaylistByIdAsync_PlaylistOwnedByAnotherUser_ReturnsForbiddenFailure()
        {
            // Arrange
            var playlist = new Playlist { Id = 5, Name = "Private", UserId = "owner", PlaylistSongs = new List<PlaylistSong>() };
            _playlistRepository.Setup(r => r.GetByIdWithSongsAsync(5)).ReturnsAsync(playlist);

            // Act
            var result = await _sut.GetPlaylistByIdAsync("someone-else", 5);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.Forbidden);
        }

        // 5. Update playlist successfully.
        [Fact]
        public async Task UpdatePlaylistAsync_ValidOwner_UpdatesNameAndDescription()
        {
            // Arrange
            const string userId = "user-1";
            var playlist = new Playlist { Id = 7, Name = "Old Name", UserId = userId, PlaylistSongs = new List<PlaylistSong>() };
            _playlistRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(playlist);

            var dto = new UpdatePlaylistDto { Name = "New Name", Description = "New description" };

            // Act
            var result = await _sut.UpdatePlaylistAsync(userId, 7, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.Name.Should().Be("New Name");
            result.Value.Description.Should().Be("New description");
            _playlistRepository.Verify(r => r.Update(playlist), Times.Once);
            _playlistRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // 6. Delete playlist successfully.
        [Fact]
        public async Task DeletePlaylistAsync_ValidOwner_RemovesPlaylistAndDeletesCoverImage()
        {
            // Arrange
            const string userId = "user-1";
            var playlist = new Playlist
            {
                Id = 9,
                Name = "To Delete",
                UserId = userId,
                CoverImagePath = "uploads/coverPath/abc.png",
                PlaylistSongs = new List<PlaylistSong>()
            };
            _playlistRepository.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(playlist);

            // Act
            var result = await _sut.DeletePlaylistAsync(userId, 9);

            // Assert: cover file cleanup happens, then the row is removed.
            result.IsSuccess.Should().BeTrue();
            _fileStorageService.Verify(f => f.DeleteFile("uploads/coverPath/abc.png"), Times.Once);
            _playlistRepository.Verify(r => r.Remove(playlist), Times.Once);
            _playlistRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // 7. Delete non-existing playlist fails with NotFound.
        [Fact]
        public async Task DeletePlaylistAsync_PlaylistDoesNotExist_ReturnsNotFoundFailure()
        {
            // Arrange
            _playlistRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Playlist?)null);

            // Act
            var result = await _sut.DeletePlaylistAsync("user-1", 999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.NotFound);
            _playlistRepository.Verify(r => r.Remove(It.IsAny<Playlist>()), Times.Never);
        }

        // Corresponds to the spec's "Add existing song to another playlist
        // successfully" (SongService list, item 7) — the actual operation
        // lives on PlaylistService since it's the playlist that owns the
        // PlaylistSong association, not the song.
        [Fact]
        public async Task AddSongToPlaylistAsync_SongAlreadyBelongsToAnotherPlaylist_AddsToThisPlaylistSuccessfully()
        {
            // Arrange
            const string userId = "user-1";
            var playlist = new Playlist { Id = 3, Name = "Second Playlist", UserId = userId, PlaylistSongs = new List<PlaylistSong>() };
            var song = new Song { Id = 42, Title = "Reused Song", Artist = "Someone" };

            _playlistRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(playlist);
            _songRepository.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(song);
            // Not already in *this* playlist — it doesn't matter that it's in another one.
            _playlistRepository.Setup(r => r.GetPlaylistSongAsync(3, 42)).ReturnsAsync((PlaylistSong?)null);
            _playlistRepository.Setup(r => r.GetNextSongOrderAsync(3)).ReturnsAsync(1);

            var dto = new AddSongToPlaylistDto { SongId = 42 };

            // Act
            var result = await _sut.AddSongToPlaylistAsync(userId, 3, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _playlistRepository.Verify(r => r.AddSongAsync(It.Is<PlaylistSong>(ps => ps.PlaylistId == 3 && ps.SongId == 42)), Times.Once);
            _playlistRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // Edge case: adding a song already in the same playlist is rejected.
        [Fact]
        public async Task AddSongToPlaylistAsync_SongAlreadyInThisPlaylist_ReturnsBadRequestFailure()
        {
            // Arrange
            const string userId = "user-1";
            var playlist = new Playlist { Id = 3, Name = "Playlist", UserId = userId, PlaylistSongs = new List<PlaylistSong>() };
            var song = new Song { Id = 42, Title = "Song", Artist = "Artist" };
            var existingAssociation = new PlaylistSong { PlaylistId = 3, SongId = 42 };

            _playlistRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(playlist);
            _songRepository.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(song);
            _playlistRepository.Setup(r => r.GetPlaylistSongAsync(3, 42)).ReturnsAsync(existingAssociation);

            var dto = new AddSongToPlaylistDto { SongId = 42 };

            // Act
            var result = await _sut.AddSongToPlaylistAsync(userId, 3, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.BadRequest);
            _playlistRepository.Verify(r => r.AddSongAsync(It.IsAny<PlaylistSong>()), Times.Never);
        }

        // Edge case: duplicate playlist name is rejected without throwing.
        [Fact]
        public async Task CreatePlaylistAsync_DuplicateName_ReturnsBadRequestFailure()
        {
            // Arrange
            const string userId = "user-1";
            var dto = new CreatePlaylistDto { Name = "Road Trip" };

            _playlistRepository.Setup(r => r.ExistsByNameAsync(userId, dto.Name, null)).ReturnsAsync(true);

            // Act
            var result = await _sut.CreatePlaylistAsync(userId, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.BadRequest);
            _playlistRepository.Verify(r => r.AddAsync(It.IsAny<Playlist>()), Times.Never);
        }
    }
}
