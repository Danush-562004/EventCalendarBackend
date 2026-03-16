import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResponse, VenueResponse } from '../models/api.model';

@Injectable({ providedIn: 'root' })
export class VenueService {
    private readonly base = `${environment.apiUrl}/venues`;

    constructor(private http: HttpClient) { }

    getAll(page = 1, pageSize = 20) {
        const params = new HttpParams().set('page', page).set('pageSize', pageSize);
        return this.http.get<ApiResponse<PagedResponse<VenueResponse>>>(this.base, { params });
    }

    getById(id: number) {
        return this.http.get<ApiResponse<VenueResponse>>(`${this.base}/${id}`);
    }

    create(data: any) {
        return this.http.post<ApiResponse<VenueResponse>>(this.base, data);
    }

    update(id: number, data: any) {
        return this.http.put<ApiResponse<VenueResponse>>(`${this.base}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete<ApiResponse<null>>(`${this.base}/${id}`);
    }
}
