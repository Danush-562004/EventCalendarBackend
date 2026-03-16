import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    {
        path: 'auth',
        canActivate: [guestGuard],
        children: [
            { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
            { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
            { path: '', redirectTo: 'login', pathMatch: 'full' }
        ]
    },
    {
        path: 'dashboard',
        canActivate: [authGuard],
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
    },
    {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
    },
    {
        path: 'events',
        children: [
            { path: '', loadComponent: () => import('./features/events/event-list/event-list.component').then(m => m.EventListComponent) },
            { path: 'create', canActivate: [authGuard], loadComponent: () => import('./features/events/event-form/event-form.component').then(m => m.EventFormComponent) },
            { path: ':id', loadComponent: () => import('./features/events/event-detail/event-detail.component').then(m => m.EventDetailComponent) },
            { path: ':id/edit', canActivate: [authGuard], loadComponent: () => import('./features/events/event-form/event-form.component').then(m => m.EventFormComponent) }
        ]
    },
    {
        path: 'reminders',
        canActivate: [authGuard],
        children: [
            { path: '', loadComponent: () => import('./features/reminders/reminder-list/reminder-list.component').then(m => m.ReminderListComponent) },
            { path: 'create', loadComponent: () => import('./features/reminders/reminder-form/reminder-form.component').then(m => m.ReminderFormComponent) },
            { path: ':id/edit', loadComponent: () => import('./features/reminders/reminder-form/reminder-form.component').then(m => m.ReminderFormComponent) }
        ]
    },
    {
        path: 'tickets',
        canActivate: [authGuard],
        loadComponent: () => import('./features/tickets/ticket-list/ticket-list.component').then(m => m.TicketListComponent)
    },
    {
        path: 'venues',
        canActivate: [authGuard],
        children: [
            { path: '', loadComponent: () => import('./features/venues/venue-list/venue-list.component').then(m => m.VenueListComponent) },
            { path: 'create', canActivate: [adminGuard], loadComponent: () => import('./features/venues/venue-form/venue-form.component').then(m => m.VenueFormComponent) },
            { path: ':id/edit', canActivate: [adminGuard], loadComponent: () => import('./features/venues/venue-form/venue-form.component').then(m => m.VenueFormComponent) }
        ]
    },
    {
        path: 'categories',
        canActivate: [authGuard],
        children: [
            { path: '', loadComponent: () => import('./features/categories/category-list/category-list.component').then(m => m.CategoryListComponent) },
            { path: 'create', canActivate: [adminGuard], loadComponent: () => import('./features/categories/category-form/category-form.component').then(m => m.CategoryFormComponent) },
            { path: ':id/edit', canActivate: [adminGuard], loadComponent: () => import('./features/categories/category-form/category-form.component').then(m => m.CategoryFormComponent) }
        ]
    },
    { path: '**', redirectTo: 'dashboard' }
];
