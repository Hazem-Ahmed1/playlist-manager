using Microsoft.AspNetCore.Identity;
using PlaylistManagement.Api.Common;
using PlaylistManagement.Api.DTOs.Auth;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Services
{
    /// <inheritdoc cref="IAuthService" />
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
            {
                return Result<AuthResponseDto>.Failure(ErrorType.Conflict, "An account with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var identityResult = await _userManager.CreateAsync(user, dto.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(" ", identityResult.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure(ErrorType.BadRequest, errors);
            }

            // Every self-registered account is a plain user; Admin is only
            // ever granted by seeding/manual assignment, never through this
            // public endpoint.
            await _userManager.AddToRoleAsync(user, Roles.User);

            var response = await BuildAuthResponseAsync(user);
            return Result<AuthResponseDto>.Success(response);
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                return Result<AuthResponseDto>.Failure(ErrorType.Unauthorized, "Invalid email or password.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Result<AuthResponseDto>.Failure(ErrorType.Unauthorized, "Invalid email or password.");
            }

            var response = await BuildAuthResponseAsync(user);
            return Result<AuthResponseDto>.Success(response);
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}
