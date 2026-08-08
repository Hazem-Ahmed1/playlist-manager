import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Song, UpdateSongRequest, UploadSongRequest } from '../models/song.model';

/** Calls the /api/songs endpoints — browsing is public, upload/delete are Admin-only (enforced server-side; the UI just hides them). */
@Injectable({ providedIn: 'root' })
export class SongService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/songs`;

  getAll(): Observable<Song[]> {
    return this.http.get<ApiResponse<Song[]>>(this.baseUrl).pipe(map((res) => res.data));
  }

  getById(id: number): Observable<Song> {
    return this.http.get<ApiResponse<Song>>(`${this.baseUrl}/${id}`).pipe(map((res) => res.data));
  }

  upload(payload: UploadSongRequest): Observable<Song> {
    const formData = new FormData();
    formData.append('Title', payload.title);
    formData.append('Artist', payload.artist);
    if (payload.album) {
      formData.append('Album', payload.album);
    }
    if (payload.genre) {
      formData.append('Genre', payload.genre);
    }
    if (payload.duration) {
      formData.append('Duration', payload.duration);
    }
    formData.append('File', payload.file);

    return this.http.post<ApiResponse<Song>>(this.baseUrl, formData).pipe(map((res) => res.data));
  }

  update(id: number, payload: UpdateSongRequest): Observable<Song> {
    return this.http.put<ApiResponse<Song>>(`${this.baseUrl}/${id}`, payload).pipe(map((res) => res.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
