import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-category-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="page">
        <div class="page__header">
            <h1>{{ isEdit ? 'Edit Category' : 'Add Category' }}</h1>
            <a routerLink="/categories" class="btn btn--outline">← Back</a>
        </div>
        <div class="form-card">
            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <div class="form-group">
                    <label>Name *</label>
                    <input formControlName="name" type="text" placeholder="Category name" />
                    @if (form.get('name')?.invalid && form.get('name')?.touched) {
                        <span class="form-error">Name is required.</span>
                    }
                </div>
                <div class="form-group">
                    <label>Description</label>
                    <textarea formControlName="description" rows="3" placeholder="Category description"></textarea>
                </div>
                <div class="form-group">
                    <label>Color Code</label>
                    <div class="color-row">
                        <input formControlName="colorCode" type="color" class="color-picker" />
                        <input formControlName="colorCode" type="text" placeholder="#4f46e5" class="color-text" />
                    </div>
                </div>
                <div class="form-actions">
                    <a routerLink="/categories" class="btn btn--outline">Cancel</a>
                    <button type="submit" class="btn btn--primary" [disabled]="loading()">
                        {{ loading() ? 'Saving...' : (isEdit ? 'Update' : 'Create') }}
                    </button>
                </div>
            </form>
        </div>
    </div>
    `,
    styles: [`
        .page { padding: 2rem; max-width: 600px; margin: 0 auto; }
        .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page__header h1 { font-size: 1.75rem; color: var(--dark); }
        .form-card { background: var(--card); border-radius: 12px; padding: 2rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); display: flex; flex-direction: column; gap: 1.25rem; }
        .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
        .form-group label { font-size: 0.875rem; font-weight: 600; color: var(--dark); }
        .form-group input[type="text"], .form-group textarea {
            padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; font-family: inherit;
        }
        .form-group input:focus, .form-group textarea:focus { border-color: var(--primary); }
        .color-row { display: flex; align-items: center; gap: 0.75rem; }
        .color-picker { width: 48px; height: 40px; border: 1.5px solid var(--border); border-radius: 8px; padding: 2px; cursor: pointer; }
        .color-text { flex: 1; padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; }
        .color-text:focus { border-color: var(--primary); }
        .form-error { color: var(--danger); font-size: 0.8rem; }
        .form-actions { display: flex; justify-content: flex-end; gap: 1rem; padding-top: 1rem; border-top: 1px solid var(--border); }
        .btn { padding: 0.6rem 1.25rem; border-radius: 6px; text-decoration: none; font-size: 0.9rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
        .btn--primary { background: var(--primary); color: #fff; }
        .btn--primary:hover:not(:disabled) { background: var(--primary-dark); text-decoration: none; }
        .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
        .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
    `]
})
export class CategoryFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private categoryService = inject(CategoryService);
    private toast = inject(ToastService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    loading = signal(false);
    isEdit = false;
    categoryId?: number;

    form = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        colorCode: ['#4f46e5']
    });

    ngOnInit() {
        this.categoryId = this.route.snapshot.params['id'];
        if (this.categoryId) {
            this.isEdit = true;
            this.categoryService.getById(this.categoryId).subscribe(res => this.form.patchValue(res.data as any));
        }
    }

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading.set(true);
        const obs = this.isEdit
            ? this.categoryService.update(this.categoryId!, this.form.value)
            : this.categoryService.create(this.form.value);

        obs.subscribe({
            next: () => {
                this.toast.success(this.isEdit ? 'Category updated.' : 'Category created.');
                this.router.navigate(['/categories']);
            },
            error: err => {
                this.toast.error(err.error?.message || 'Failed to save category.');
                this.loading.set(false);
            }
        });
    }
}
