import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CreateJiraIssueRequest,
  JiraPocService,
  JiraPocTicket
} from '../../jira-poc.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-jira-poc',
  imports: [CommonModule, FormsModule],
  templateUrl: './jira-poc.component.html',
  styleUrl: './jira-poc.component.scss'
})
export class JiraPocComponent implements OnInit {
  private jiraPocService = inject(JiraPocService);

  readonly jiraBaseUrl = environment.jiraBaseUrl;

  title = '';
  description = '';
  priority = 'Medium';
  issueType = 'Task';

  lastTicket: JiraPocTicket | null = null;
  tickets: JiraPocTicket[] = [];
  errorMessage = '';
  loading = false;
  listLoading = false;

  ngOnInit(): void {
    this.loadTickets();
  }

  createTicket(): void {
    this.errorMessage = '';
    this.loading = true;

    const body: CreateJiraIssueRequest = {
      summary: this.title.trim(),
      description: this.description.trim() || undefined,
      priority: this.priority,
      issueType: this.issueType
    };

    this.jiraPocService.createIssue(body).subscribe({
      next: (ticket) => {
        this.lastTicket = ticket;
        this.loading = false;
        this.loadTickets();
      },
      error: (err) => {
        this.errorMessage =
          err?.error?.title ??
          err?.error ??
          'Failed to create ticket. Is API running on http://localhost:5014?';
        this.loading = false;
      }
    });
  }

  refreshTicket(): void {
    if (!this.lastTicket?.jiraKey) {
      return;
    }

    this.errorMessage = '';
    this.jiraPocService.getIssueByKey(this.lastTicket.jiraKey).subscribe({
      next: (ticket) => {
        this.lastTicket = ticket;
        this.loadTickets();
      },
      error: () => {
        this.errorMessage = 'Failed to refresh ticket from Jira.';
      }
    });
  }

  loadTickets(): void {
    this.listLoading = true;
    this.jiraPocService.listIssues().subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.listLoading = false;
      },
      error: () => {
        this.listLoading = false;
      }
    });
  }

  selectTicket(ticket: JiraPocTicket): void {
    this.lastTicket = ticket;
  }

  jiraBrowseUrl(key: string): string {
    return `${this.jiraBaseUrl}/browse/${key}`;
  }
}
