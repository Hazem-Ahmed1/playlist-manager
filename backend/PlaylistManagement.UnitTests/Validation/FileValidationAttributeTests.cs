using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PlaylistManagement.Api.Validation;
using Xunit;

namespace PlaylistManagement.UnitTests.Validation
{
    /// <summary>
    /// Covers the spec's "upload invalid file type" / "upload file larger
    /// than allowed limit" scenarios directly against the Data Annotation
    /// attributes that own that rule (AllowedExtensionsAttribute,
    /// MaxFileSizeAttribute) — this is where the check actually lives,
    /// ahead of SongService (see SongServiceTests' class doc).
    /// </summary>
    public class FileValidationAttributeTests
    {
        private static IFormFile CreateFile(string fileName, long length)
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(length);
            return file.Object;
        }

        private static readonly ValidationContext Context = new(new object());

        // 2. Upload invalid file type throws a validation error.
        [Fact]
        public void AllowedExtensionsAttribute_DisallowedExtension_ReturnsValidationError()
        {
            // Arrange
            var attribute = new AllowedExtensionsAttribute(new[] { ".mp3", ".wav", ".m4a" });
            var file = CreateFile("malware.exe", 1024);

            // Act
            var result = attribute.GetValidationResult(file, Context);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
        }

        [Fact]
        public void AllowedExtensionsAttribute_AllowedExtension_ReturnsSuccess()
        {
            // Arrange
            var attribute = new AllowedExtensionsAttribute(new[] { ".mp3", ".wav", ".m4a" });
            var file = CreateFile("track.mp3", 1024);

            // Act
            var result = attribute.GetValidationResult(file, Context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        // 3. Upload file larger than allowed limit throws a validation error.
        [Fact]
        public void MaxFileSizeAttribute_FileExceedsLimit_ReturnsValidationError()
        {
            // Arrange
            const long maxBytes = 20 * 1024 * 1024;
            var attribute = new MaxFileSizeAttribute(maxBytes);
            var file = CreateFile("huge.mp3", maxBytes + 1);

            // Act
            var result = attribute.GetValidationResult(file, Context);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
        }

        [Fact]
        public void MaxFileSizeAttribute_FileWithinLimit_ReturnsSuccess()
        {
            // Arrange
            const long maxBytes = 20 * 1024 * 1024;
            var attribute = new MaxFileSizeAttribute(maxBytes);
            var file = CreateFile("normal.mp3", 1024);

            // Act
            var result = attribute.GetValidationResult(file, Context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }
    }
}
