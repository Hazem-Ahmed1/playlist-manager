using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistManagement.Api.DTOs.Common;
using PlaylistManagement.Api.DTOs.Songs;
using PlaylistManagement.Api.Interfaces;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Controllers
{
    /// <summary>
    /// Browsing the song catalog is public — the site displays and plays
    /// songs to anonymous visitors. Only building playlists from them
    /// requires auth (see PlaylistsController), and only Admins can add new
    /// songs to the catalog.
    /// </summary>
    [Route("api/songs")]
    public class SongsController : ApiControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        /// <summary>Gets every song in the catalog. Public — no authentication required.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var songs = await _songService.GetAllSongsAsync();

            return Ok(ApiResponse<IReadOnlyList<SongDto>>.Ok(songs));
        }

        /// <summary>Gets a single song by id. Public — no authentication required.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _songService.GetSongByIdAsync(id);
            if (!result.IsSuccess)
            {
                return FromError(result.ErrorType, result.ErrorMessage!);
            }

            return Ok(ApiResponse<SongDto>.Ok(result.Value!));
        }

        /// <summary>Uploads a new song to the catalog. Requires the Admin role.</summary>
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Upload([FromForm] UploadSongDto dto)
        {
            var song = await _songService.UploadSongAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = song.Id },
                ApiResponse<SongDto>.Ok(song, "Song uploaded successfully."));
        }

        /// <summary>Updates a song's metadata. Requires the Admin role.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSongDto dto)
        {
            var result = await _songService.UpdateSongAsync(id, dto);
            if (!result.IsSuccess)
            {
                return FromError(result.ErrorType, result.ErrorMessage!);
            }

            return Ok(ApiResponse<SongDto>.Ok(result.Value!, "Song updated successfully."));
        }

        /// <summary>Removes a song from the catalog. Requires the Admin role.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _songService.DeleteSongAsync(id);
            if (!result.IsSuccess)
            {
                return FromError(result.ErrorType, result.ErrorMessage!);
            }

            return NoContent();
        }
    }
}
