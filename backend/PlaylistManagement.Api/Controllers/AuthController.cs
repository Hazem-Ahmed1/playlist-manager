using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistManagement.Api.DTOs.Auth;
using PlaylistManagement.Api.DTOs.Common;
using PlaylistManagement.Api.Interfaces;

namespace PlaylistManagement.Api.Controllers
{
    /// <summary>Account registration and login. Issues a JWT to be sent as a Bearer token on subsequent requests.</summary>
    [AllowAnonymous]
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Creates a new account and returns a JWT.</summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = await _authService.RegisterAsync(dto);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthResponseDto>.Ok(response, "Account created successfully."));
        }

        /// <summary>Verifies credentials and returns a JWT.</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
        }
    }
}
