import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse, CategoryResponse, PagedResponse } from '../models/api.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
    private readonly base = `${environment.apiUrl}/categories`;

    constructor(private http: HttpClient) { }

    getAll(page = 1, pageSize = 100) {
        return this.http.get<ApiResponse<PagedResponse<CategoryResponse>>>(`${this.base}?page=${page}&pageSize=${pageSize}`);
    }

    getById(id: number) {
        return this.http.get<ApiResponse<CategoryResponse>>(`${this.base}/${id}`);
    }

    create(data: any) {
        return this.http.post<ApiResponse<CategoryResponse>>(this.base, data);
    }

    update(id: number, data: any) {
        return this.http.put<ApiResponse<CategoryResponse>>(`${this.base}/${id}`, data);
    }

    delete(id: number) {
        return this.http.delete<ApiResponse<null>>(`${this.base}/${id}`);
    }
}
