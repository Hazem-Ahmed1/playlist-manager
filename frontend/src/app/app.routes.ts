import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/layout').then((m) => m.Layout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/home').then((m) => m.Home),
      },
      {
        path: 'songs',
        loadComponent: () => import('./features/songs/songs-catalog').then((m) => m.SongsCatalog),
      },
      {
        path: 'playlists',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/playlists/playlist-list/playlist-list').then((m) => m.PlaylistList),
      },
      {
        path: 'playlists/:id',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/playlists/playlist-detail/playlist-detail').then((m) => m.PlaylistDetail),
      },
      {
        path: 'catalog',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/catalog/catalog-dashboard/catalog-dashboard').then((m) => m.CatalogDashboard),
      },
    ],
  },
  {
    path: '403',
    loadComponent: () => import('./features/errors/forbidden/forbidden').then((m) => m.Forbidden),
  },
  {
    path: '**',
    loadComponent: () => import('./features/errors/not-found/not-found').then((m) => m.NotFound),
  },
];
