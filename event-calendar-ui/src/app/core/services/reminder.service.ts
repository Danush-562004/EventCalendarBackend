import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResponse, ReminderResponse } from '../models/api.model';

@Injectable({ providedIn: 'root' })
export class ReminderService {
    private readonly base = `${environment.apiUrl}/reminders`;

    constructor(private http: HttpClient) { }

    getMyReminders(page = 1, pageSize = 20) {
        const params = new HttpParams().set('page', page).set('pageSize', pageSize);
        return this.http.get<ApiResponse<PagedResponse<ReminderResponse>>>(this.base, { params });
    }

    getById(id: number) {
        return this.http.get<ApiResponse<ReminderResponse>>(`${this.base}/${id}`);
    }

    getByEvent(eventId: number) {
        return this.http.get<ApiResponse<ReminderResponse[]>>(`${this.base}/by-event/${eventId}`);
    }

    create(data: any) {
        return this.http.post<ApiResponse<ReminderResponse>>(this.base, data);
    }

    update(id: number, data: any) {
        return this.http.put<ApiResponse<ReminderResponse>>(`${this.base}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete<ApiResponse<null>>(`${this.base}/${id}`);
    }
}
