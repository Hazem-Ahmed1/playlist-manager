/** Mirrors PlaylistManagement.Api.DTOs.Songs.SongDto. */
export interface Song {
  id: number;
  title: string;
  artist: string;
  album: string | null;
  genre: string | null;
  duration: string;
  filePath: string;
  fileSize: number;
  uploadedAt: string;
}

/**
 * Everything the bottom audio player needs to play and display a track.
 * Deliberately narrower than Song — PlaylistSongItem (a song's entry
 * inside a playlist) doesn't carry genre/fileSize/uploadedAt, and the
 * player has no use for them anyway. Both Song and PlaylistSongItem
 * satisfy this structurally.
 */
export interface PlayableSong {
  id: number;
  title: string;
  artist: string;
  duration: string;
  filePath: string;
}

/**
 * Mirrors PlaylistManagement.Api.DTOs.Songs.UploadSongDto's non-file fields.
 * The File itself is appended separately when building the multipart
 * FormData — see SongService.uploadSong.
 */
export interface UploadSongRequest {
  title: string;
  artist: string;
  album?: string | null;
  genre?: string | null;
  duration?: string | null;
  file: File;
}

/** Mirrors PlaylistManagement.Api.DTOs.Songs.UpdateSongDto. No file — metadata only. */
export interface UpdateSongRequest {
  title: string;
  artist: string;
  album?: string | null;
  genre?: string | null;
  duration?: string | null;
}
