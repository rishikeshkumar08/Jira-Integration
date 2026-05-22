import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class HttpServicesService {
  constructor(private http: HttpClient) {}

  post(url: string, obj: unknown): Observable<unknown> {
    return this.http.post(url, obj);
  }

  get(url: string): Observable<unknown> {
    return this.http.get(url);
  }
}
