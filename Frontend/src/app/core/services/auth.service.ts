import { Injectable, signal, effect, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserResponse } from '../models/auth.model';
import { ApiResponse } from '../models/api.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly TOKEN_KEY = 'ec_token';
    private readonly USER_KEY = 'ec_user';

    private _token = signal<string | null>(localStorage.getItem(this.TOKEN_KEY));
    private _user = signal<UserResponse | null>(this.loadUser());

    readonly token = this._token.asReadonly();
    readonly currentUser = this._user.asReadonly();
    readonly isLoggedIn = computed(() => !!this._token());
    readonly isAdmin = computed(() => this._user()?.role === 'Admin');

    constructor(private http: HttpClient, private router: Router) {
        effect(() => {
            const t = this._token();
            if (t) localStorage.setItem(this.TOKEN_KEY, t);
            else localStorage.removeItem(this.TOKEN_KEY);
        });

        effect(() => {
            const u = this._user();
            if (u) localStorage.setItem(this.USER_KEY, JSON.stringify(u));
            else localStorage.removeItem(this.USER_KEY);
        });
    }

    login(request: LoginRequest) {
        return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, request).pipe(
            tap(res => {
                if (res.success) {
                    this._token.set(res.data.token);
                    this._user.set(res.data.user);
                }
            })
        );
    }

    register(request: RegisterRequest) {
        return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/register`, request).pipe(
            tap(res => {
                if (res.success) {
                    this._token.set(res.data.token);
                    this._user.set(res.data.user);
                }
            })
        );
    }

    logout() {
        this._token.set(null);
        this._user.set(null);
        this.router.navigate(['/auth/login']);
    }

    private loadUser(): UserResponse | null {
        const raw = localStorage.getItem(this.USER_KEY);
        return raw ? JSON.parse(raw) : null;
    }
}
