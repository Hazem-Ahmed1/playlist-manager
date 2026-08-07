import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  AddSongToPlaylistRequest,
  CreatePlaylistRequest,
  Playlist,
  PlaylistDetail,
  UpdatePlaylistRequest,
} from '../models/playlist.model';

/** Calls the /api/playlists endpoints — the current user's playlists and their song membership. */
@Injectable({ providedIn: 'root' })
export class PlaylistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/playlists`;

  getMyPlaylists(): Observable<Playlist[]> {
    return this.http.get<ApiResponse<Playlist[]>>(this.baseUrl).pipe(map((res) => res.data));
  }

  getById(id: number): Observable<PlaylistDetail> {
    return this.http.get<ApiResponse<PlaylistDetail>>(`${this.baseUrl}/${id}`).pipe(map((res) => res.data));
  }

  create(payload: CreatePlaylistRequest): Observable<Playlist> {
    return this.http.post<ApiResponse<Playlist>>(this.baseUrl, payload).pipe(map((res) => res.data));
  }

  update(id: number, payload: UpdatePlaylistRequest): Observable<Playlist> {
    return this.http.put<ApiResponse<Playlist>>(`${this.baseUrl}/${id}`, payload).pipe(map((res) => res.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  addSong(playlistId: number, songId: number): Observable<void> {
    const body: AddSongToPlaylistRequest = { songId };
    return this.http
      .post<ApiResponse<null>>(`${this.baseUrl}/${playlistId}/songs`, body)
      .pipe(map(() => undefined));
  }

  removeSong(playlistId: number, songId: number): Observable<void> {
    return this.http
      .delete<ApiResponse<null>>(`${this.baseUrl}/${playlistId}/songs/${songId}`)
      .pipe(map(() => undefined));
  }
}
