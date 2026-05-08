import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface SmtpProfile {
  id: string;
  user: string;
  fromEmail: string;
  displayName: string;
}

export interface CreateSmtpProfile {
  host: string;
  port: number;
  user: string;
  password: string;
  fromEmail: string;
  displayName: string;
}

@Injectable({ providedIn: 'root' })
export class SmtpService {
  private readonly url = '/api/smtp';

  constructor(private http: HttpClient) {}

  getProfiles(): Observable<SmtpProfile[]> {
    return this.http.get<{ smtpProfiles: SmtpProfile[] }>(this.url).pipe(
      map(r => r.smtpProfiles ?? [])
    );
  }

  addProfile(profile: CreateSmtpProfile): Observable<void> {
    return this.http.post<void>(this.url, profile);
  }

  deleteProfile(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
