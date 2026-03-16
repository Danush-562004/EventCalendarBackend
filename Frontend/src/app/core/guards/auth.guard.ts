import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const authGuard: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const toast = inject(ToastService);

    if (auth.isLoggedIn()) return true;

    toast.warning('Please log in to access this page.');
    return router.createUrlTree(['/auth/login']);
};

export const adminGuard: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const toast = inject(ToastService);

    if (auth.isAdmin()) return true;

    toast.error('Access denied. Admin privileges required.');
    return router.createUrlTree(['/dashboard']);
};

export const guestGuard: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) return true;
    return router.createUrlTree(['/dashboard']);
};
