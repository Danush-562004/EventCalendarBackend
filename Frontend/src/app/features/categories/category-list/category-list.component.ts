import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { CategoryResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-category-list',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page">
        <div class="page__header">
            <h1>Categories</h1>
            @if (auth.isAdmin()) {
                <a routerLink="/categories/create" class="btn btn--primary">+ Add Category</a>
            }
        </div>
        @if (loading()) {
            <div class="loading">Loading categories...</div>
        } @else if (categories().length === 0) {
            <div class="empty-state">
                <span>🏷️</span>
                <p>No categories found.</p>
            </div>
        } @else {
            <div class="category-grid">
                @for (cat of categories(); track cat.id) {
                    <div class="category-card" [style.border-left-color]="cat.colorCode">
                        <div class="category-card__color" [style.background]="cat.colorCode"></div>
                        <div class="category-card__body">
                            <h3>{{ cat.name }}</h3>
                            @if (cat.description) { <p>{{ cat.description }}</p> }
                            @if (!cat.isActive) { <span class="inactive-badge">Inactive</span> }
                        </div>
                        @if (auth.isAdmin()) {
                            <div class="category-card__actions">
                                <a [routerLink]="['/categories', cat.id, 'edit']" class="btn btn--sm btn--warning">Edit</a>
                                <button class="btn btn--sm btn--danger" (click)="deleteCategory(cat.id)">Delete</button>
                            </div>
                        }
                    </div>
                }
            </div>
            <div class="pagination">
                <button class="btn btn--outline" [disabled]="page() === 1" (click)="changePage(page() - 1)">← Prev</button>
                <span>Page {{ page() }} of {{ totalPages() }}</span>
                <button class="btn btn--outline" [disabled]="page() >= totalPages()" (click)="changePage(page() + 1)">Next →</button>
            </div>
        }
    </div>
    `,
    styles: [`
        .page { padding: 2rem; max-width: 1000px; margin: 0 auto; }
        .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page__header h1 { font-size: 1.75rem; color: var(--dark); }
        .loading { text-align: center; padding: 3rem; color: var(--muted); }
        .empty-state { text-align: center; padding: 4rem 2rem; color: var(--muted); display: flex; flex-direction: column; align-items: center; gap: 1rem; }
        .empty-state span { font-size: 3rem; }
        .category-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 1.25rem; }
        .category-card { background: var(--card); border-radius: 12px; padding: 1.25rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); border-left: 4px solid #ccc; display: flex; align-items: center; gap: 1rem; }
        .category-card__color { width: 40px; height: 40px; border-radius: 8px; flex-shrink: 0; }
        .category-card__body { flex: 1; }
        .category-card__body h3 { font-size: 1rem; font-weight: 700; color: var(--dark); margin-bottom: 0.2rem; }
        .category-card__body p { font-size: 0.85rem; color: var(--muted); }
        .inactive-badge { background: #fee2e2; color: var(--danger); padding: 0.15rem 0.5rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
        .category-card__actions { display: flex; gap: 0.5rem; flex-shrink: 0; }
        .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
        .btn--primary { background: var(--primary); color: #fff; }
        .btn--primary:hover { background: var(--primary-dark); text-decoration: none; }
        .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
        .btn--sm { padding: 0.3rem 0.65rem; font-size: 0.8rem; }
        .btn--warning { background: var(--warning); color: #fff; }
        .btn--danger { background: var(--danger); color: #fff; }
        .btn:disabled { opacity: 0.5; cursor: not-allowed; }
        .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 2rem; }
    `]
})
export class CategoryListComponent implements OnInit {
    private categoryService = inject(CategoryService);
    private toast = inject(ToastService);
    auth = inject(AuthService);

    categories = signal<CategoryResponse[]>([]);
    loading = signal(true);
    page = signal(1);
    totalPages = signal(1);

    ngOnInit() { this.loadCategories(); }

    loadCategories() {
        this.loading.set(true);
        this.categoryService.getAll(this.page(), 20).subscribe({
            next: res => {
                this.categories.set(res.data.items);
                this.totalPages.set(res.data.totalPages);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    changePage(p: number) { this.page.set(p); this.loadCategories(); }

    deleteCategory(id: number) {
        if (!confirm('Delete this category?')) return;
        this.categoryService.delete(id).subscribe({
            next: () => { this.toast.success('Category deleted.'); this.loadCategories(); },
            error: err => this.toast.error(err.error?.message || 'Failed to delete category.')
        });
    }
}
