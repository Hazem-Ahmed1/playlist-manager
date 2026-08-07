import { Playlist } from '../models/playlist.model';

/**
 * Shown on the Home page only while signed out, so first-time visitors see
 * what a playlist card looks like instead of an empty state. Never sent to
 * or received from the API — id is negative specifically so it can never
 * collide with a real playlist id.
 */
export const DEMO_PLAYLIST_ID = -1;

export const DEMO_PLAYLIST: Playlist = {
  id: DEMO_PLAYLIST_ID,
  name: 'Sample Playlist',
  description: 'This is what your playlists will look like.',
  coverImagePath: null,
  songCount: 3,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
};
