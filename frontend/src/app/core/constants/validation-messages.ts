/** Mirrors PlaylistManagement.Api.DTOs.Playlists.CreatePlaylistDto / UpdatePlaylistDto. */
export const PLAYLIST_MESSAGES = {
  nameRequired: 'Playlist name is required.',
  nameMaxLength: 'Playlist name cannot exceed 100 characters.',
  descriptionMaxLength: 'Description cannot exceed 500 characters.',
} as const;

/** Mirrors PlaylistManagement.Api.DTOs.Songs.UploadSongDto and its AllowedExtensions/MaxFileSize attributes. */
export const SONG_MESSAGES = {
  titleRequired: 'Song title is required.',
  titleMaxLength: 'Song title cannot exceed 200 characters.',
  artistRequired: 'Artist name is required.',
  artistMaxLength: 'Artist name cannot exceed 150 characters.',
  albumMaxLength: 'Album cannot exceed 150 characters.',
  genreMaxLength: 'Genre cannot exceed 100 characters.',
  fileRequired: 'Song file is required.',
  fileInvalidExtension: 'Only MP3, WAV, and M4A files are allowed.',
  fileTooLarge: 'File size cannot exceed 20 MB.',
} as const;

export const ALLOWED_SONG_EXTENSIONS = ['.mp3', '.wav', '.m4a'];
export const MAX_SONG_FILE_SIZE_BYTES = 20 * 1024 * 1024;
