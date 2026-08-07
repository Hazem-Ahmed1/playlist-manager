/** Mirrors PlaylistManagement.Api.DTOs.Playlists.PlaylistDto. */
export interface Playlist {
  id: number;
  name: string;
  description: string | null;
  coverImagePath: string | null;
  songCount: number;
  createdAt: string;
  updatedAt: string;
}

/** Mirrors PlaylistManagement.Api.DTOs.Playlists.PlaylistSongDto. */
export interface PlaylistSongItem {
  songId: number;
  title: string;
  artist: string;
  album: string | null;
  duration: string;
  filePath: string;
  order: number;
  addedAt: string;
}

/** Mirrors PlaylistManagement.Api.DTOs.Playlists.PlaylistDetailDto. */
export interface PlaylistDetail {
  id: number;
  name: string;
  description: string | null;
  coverImagePath: string | null;
  createdAt: string;
  updatedAt: string;
  songs: PlaylistSongItem[];
}

/** The subset of playlist fields PlaylistFormModal needs to pre-fill an edit — satisfied by both Playlist and PlaylistDetail. */
export type PlaylistFormSeed = Pick<Playlist, 'id' | 'name' | 'description'>;

/** Mirrors PlaylistManagement.Api.DTOs.Playlists.CreatePlaylistDto. */
export interface CreatePlaylistRequest {
  name: string;
  description?: string | null;
}

/** Mirrors PlaylistManagement.Api.DTOs.Playlists.UpdatePlaylistDto. */
export interface UpdatePlaylistRequest {
  name: string;
  description?: string | null;
}

/** Mirrors PlaylistManagement.Api.DTOs.Playlists.AddSongToPlaylistDto. */
export interface AddSongToPlaylistRequest {
  songId: number;
}
