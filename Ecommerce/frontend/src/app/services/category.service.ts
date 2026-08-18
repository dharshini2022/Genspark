import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { Category, CategoryTreeNode } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = `${environment.baseUrl}/Category`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/list`);
  }

  getCategoryTree(): Observable<CategoryTreeNode[]> {
    return this.http.get<CategoryTreeNode[]>(this.baseUrl);
  }

  createCategory(request: { name: string; slug: string; parentId?: number }): Observable<any> {
    return this.http.post<any>(this.baseUrl, request);
  }

  updateCategory(id: number, request: { name?: string; slug?: string; parentId?: number; isActive?: boolean }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, request);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }
}
