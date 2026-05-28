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
  projectKey?: string;
  sprintId?: number;
  acceptanceCriteria?: string;
  riceScore?: number;
  kanoClarification?: string;
}

export interface JiraProject {
  id: string;
  key: string;
  name: string;
}

export interface JiraSprint {
  id: number;
  name: string;
  state: string;
}

@Injectable({ providedIn: 'root' })
export class JiraPocService {
  private readonly apiUrl = environment.apiUrl;
  private readonly issuesUrl = `${this.apiUrl}jira/issues/`;
  private readonly projectsUrl = `${this.apiUrl}jira/projects`;

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

  getProjects(): Observable<JiraProject[]> {
    return this.api.get(this.projectsUrl) as Observable<JiraProject[]>;
  }

  getSprints(projectKey: string): Observable<JiraSprint[]> {
    return this.api.get(`${this.projectsUrl}/${encodeURIComponent(projectKey)}/sprints`) as Observable<JiraSprint[]>;
  }
}
