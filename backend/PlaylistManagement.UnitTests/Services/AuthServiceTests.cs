using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PlaylistManagement.Api.DTOs.Auth;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Middleware.Exceptions;
using PlaylistManagement.Api.Models;
using PlaylistManagement.Api.Services;
using PlaylistManagement.UnitTests.TestHelpers;
using Xunit;

namespace PlaylistManagement.UnitTests.Services
{
    /// <summary>
    /// AuthService unit tests. UserManager/SignInManager are mocked via
    /// IdentityMockFactory (Identity's own testability seam) rather than
    /// touching a real user store or database.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userManager = IdentityMockFactory.CreateUserManagerMock();
            _signInManager = IdentityMockFactory.CreateSignInManagerMock(_userManager.Object);
            _sut = new AuthService(_userManager.Object, _signInManager.Object, _tokenService.Object);
        }

        // 1. Register new user successfully.
        [Fact]
        public async Task RegisterAsync_NewUser_ReturnsAuthResponseWithToken()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Password = "Str0ng!Pass"
            };

            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.User))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { Roles.User });

            _tokenService
                .Setup(t => t.GenerateToken(It.IsAny<ApplicationUser>(), It.Is<IList<string>>(r => r.Contains(Roles.User))))
                .Returns(("signed.jwt.token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert: a token comes back and the account was created as a
            // plain User (never self-registered as Admin).
            result.Token.Should().Be("signed.jwt.token");
            result.Email.Should().Be(dto.Email);
            _userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u => u.Email == dto.Email), dto.Password), Times.Once);
            _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.User), Times.Once);
        }

        // 2. Duplicate email registration fails.
        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Password = "Str0ng!Pass"
            };
            var existingUser = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            // Act
            var act = async () => await _sut.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
            _userManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        // Edge case: Identity itself rejects the account (e.g. password
        // policy failure surfaced by the store), which the service should
        // translate rather than let bubble as an IdentityResult.
        [Fact]
        public async Task RegisterAsync_IdentityCreateFails_ThrowsBadRequestException()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Password = "Str0ng!Pass"
            };

            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

            // Act
            var act = async () => await _sut.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        // 3. Login with valid credentials returns JWT.
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwt()
        {
            // Arrange
            var dto = new LoginDto { Email = "ada@example.com", Password = "Str0ng!Pass" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManager
                .Setup(s => s.CheckPasswordSignInAsync(user, dto.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { Roles.User });
            _tokenService
                .Setup(t => t.GenerateToken(user, It.IsAny<IList<string>>()))
                .Returns(("signed.jwt.token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            result.Token.Should().Be("signed.jwt.token");
        }

        // 4. Login with invalid password fails.
        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new LoginDto { Email = "ada@example.com", Password = "WrongPassword1!" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManager
                .Setup(s => s.CheckPasswordSignInAsync(user, dto.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var act = async () => await _sut.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // 5. Login with unknown email fails.
        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new LoginDto { Email = "ghost@example.com", Password = "Whatever1!" };
            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var act = async () => await _sut.LoginAsync(dto);

            // Assert: same generic message/exception as a wrong password —
            // never reveal whether the email exists.
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _signInManager.Verify(s => s.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
