import { inject, Injectable } from '@angular/core';
import { HttpServicesService } from '@core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface JiraPocTicket {
  id: number;
  title: string;
  description: string;
  priority: string;
  jiraKey: string;
  status: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateJiraIssueRequest {
  summary: string;
  description?: string;
  priority: string;
  issueType?: string;
}

@Injectable({ providedIn: 'root' })
export class JiraPocService {
  private readonly apiUrl = environment.apiUrl;
  private readonly issuesUrl = `${this.apiUrl}jira/issues/`;

  private api = inject(HttpServicesService);

  createIssue(request: CreateJiraIssueRequest): Observable<JiraPocTicket> {
    return this.api.post(this.issuesUrl, request) as Observable<JiraPocTicket>;
  }

  listIssues(): Observable<JiraPocTicket[]> {
    return this.api.get(this.issuesUrl) as Observable<JiraPocTicket[]>;
  }

  getIssueByKey(key: string): Observable<JiraPocTicket> {
    return this.api.get(this.issuesUrl + encodeURIComponent(key)) as Observable<JiraPocTicket>;
  }
}
