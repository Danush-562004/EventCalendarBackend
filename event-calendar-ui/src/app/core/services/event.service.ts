import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, EventResponse, PagedResponse } from '../models/api.model';

@Injectable({ providedIn: 'root' })
export class EventService {
    private readonly base = `${environment.apiUrl}/events`;

    constructor(private http: HttpClient) { }

    getAll(page = 1, pageSize = 12) {
        const params = new HttpParams().set('page', page).set('pageSize', pageSize);
        return this.http.get<ApiResponse<PagedResponse<EventResponse>>>(this.base, { params });
    }

    search(filter: Record<string, any>) {
        let params = new HttpParams();
        Object.entries(filter).forEach(([k, v]) => { if (v != null && v !== '') params = params.set(k, v); });
        return this.http.get<ApiResponse<PagedResponse<EventResponse>>>(`${this.base}/search`, { params });
    }

    getById(id: number) {
        return this.http.get<ApiResponse<EventResponse>>(`${this.base}/${id}`);
    }

    getMyEvents() {
        return this.http.get<ApiResponse<EventResponse[]>>(`${this.base}/my-events`);
    }

    create(data: any) {
        return this.http.post<ApiResponse<EventResponse>>(this.base, data);
    }

    update(id: number, data: any) {
        return this.http.put<ApiResponse<EventResponse>>(`${this.base}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete<ApiResponse<null>>(`${this.base}/${id}`);
    }
}
