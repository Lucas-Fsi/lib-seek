// src/app/services/livro.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Livro } from '../models/livro.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LivroService {
  private apiUrl = `${environment.apiUrl}/livros`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Livro[]> {
    return this.http.get<Livro[]>(this.apiUrl);
  }

  getById(id: number): Observable<Livro> {
    return this.http.get<Livro>(`${this.apiUrl}/${id}`);
  }

  create(livro: Omit<Livro, 'id'>): Observable<Livro> {
    return this.http.post<Livro>(this.apiUrl, livro);
  }

  update(id: number, livro: Livro): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, livro);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}