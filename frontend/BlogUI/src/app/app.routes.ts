import { Routes } from '@angular/router';
import { authGuard, bloggerGuard, adminGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/auth/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/auth/register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
  },
  {
    path: 'reset-password',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent),
  },
  {
    path: 'post/:slug',
    loadComponent: () => import('./pages/post/post-detail/post-detail.component').then(m => m.PostDetailComponent),
  },
  {
    path: 'profile/:username/followers',
    data: { listType: 'followers' },
    loadComponent: () =>
      import('./pages/profile/follow-list/follow-list.component').then(m => m.FollowListComponent),
  },
  {
    path: 'profile/:username/following',
    data: { listType: 'following' },
    loadComponent: () =>
      import('./pages/profile/follow-list/follow-list.component').then(m => m.FollowListComponent),
  },
  {
    path: 'profile/:username',
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
  },
  {
    path: 'editor',
    canActivate: [bloggerGuard],
    loadComponent: () => import('./pages/editor/editor.component').then(m => m.EditorComponent),
  },
  {
    path: 'editor/:id',
    canActivate: [bloggerGuard],
    loadComponent: () => import('./pages/editor/editor.component').then(m => m.EditorComponent),
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/settings/settings.component').then(m => m.SettingsComponent),
  },
  {
    path: 'settings/blocked',
    redirectTo: 'settings',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin.component').then(m => m.AdminComponent),
  },
  {
    path: 'feed',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/feed/feed.component').then(m => m.FeedComponent),
  },
  {
    path: 'tag/:slug',
    loadComponent: () => import('./pages/tag/tag.component').then(m => m.TagComponent),
  },
  {
    path: 'search',
    loadComponent: () => import('./pages/search/search.component').then(m => m.SearchComponent),
  },
  {
    path: '**',
    loadComponent: () => import('./pages/not-found/not-found.component').then(m => m.NotFoundComponent),
  },
];
