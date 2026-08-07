using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistManagement.Api.DTOs.Common;
using PlaylistManagement.Api.DTOs.Playlists;
using PlaylistManagement.Api.Interfaces;

namespace PlaylistManagement.Api.Controllers
{
    /// <summary>
    /// Endpoints for creating, browsing, updating, and deleting the current
    /// user's playlists, and managing which songs each playlist contains.
    /// Every action operates only on playlists owned by the authenticated
    /// user; ownership is enforced in PlaylistService.
    /// </summary>
    [Authorize]
    [Route("api/playlists")]
    public class PlaylistsController : ApiControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistsController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        /// <summary>Creates a new playlist owned by the current user.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreatePlaylistDto dto)
        {
            var playlist = await _playlistService.CreatePlaylistAsync(CurrentUserId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = playlist.Id },
                ApiResponse<PlaylistDto>.Ok(playlist, "Playlist created successfully."));
        }

        /// <summary>Gets every playlist owned by the current user.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPlaylists()
        {
            var playlists = await _playlistService.GetUserPlaylistsAsync(CurrentUserId);

            return Ok(ApiResponse<IReadOnlyList<PlaylistDto>>.Ok(playlists));
        }

        /// <summary>Gets a single playlist owned by the current user, including its songs.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(CurrentUserId, id);

            return Ok(ApiResponse<PlaylistDetailDto>.Ok(playlist));
        }

        /// <summary>Updates a playlist's name/description.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlaylistDto dto)
        {
            var playlist = await _playlistService.UpdatePlaylistAsync(CurrentUserId, id, dto);

            return Ok(ApiResponse<PlaylistDto>.Ok(playlist, "Playlist updated successfully."));
        }

        /// <summary>Sets or replaces a playlist's cover image.</summary>
        [HttpPost("{id:int}/cover")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCover(int id, [FromForm] UploadCoverImageDto dto)
        {
            var playlist = await _playlistService.UploadCoverImageAsync(CurrentUserId, id, dto);

            return Ok(ApiResponse<PlaylistDto>.Ok(playlist, "Cover image updated successfully."));
        }

        /// <summary>Deletes a playlist owned by the current user.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _playlistService.DeletePlaylistAsync(CurrentUserId, id);

            return NoContent();
        }

        /// <summary>Adds an existing song to a playlist owned by the current user.</summary>
        [HttpPost("{id:int}/songs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddSong(int id, [FromBody] AddSongToPlaylistDto dto)
        {
            await _playlistService.AddSongToPlaylistAsync(CurrentUserId, id, dto);

            return Ok(ApiResponse.Ok("Song added to playlist successfully."));
        }

        /// <summary>Removes a song from a playlist owned by the current user.</summary>
        [HttpDelete("{id:int}/songs/{songId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveSong(int id, int songId)
        {
            await _playlistService.RemoveSongFromPlaylistAsync(CurrentUserId, id, songId);

            return Ok(ApiResponse.Ok("Song removed from playlist successfully."));
        }
    }
}
