import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CreateJiraIssueRequest,
  JiraPocService,
  JiraPocTicket,
  JiraProject,
  JiraSprint
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

  // Form fields
  title = '';
  description = '';
  priority = 'Medium';
  issueType = 'Task';

  // Project + sprint picker
  projects: JiraProject[] = [];
  sprints: JiraSprint[] = [];
  selectedProjectKey = '';
  selectedSprintId: number | null = null;
  projectsLoading = false;
  sprintsLoading = false;

  // Ticket list
  lastTicket: JiraPocTicket | null = null;
  tickets: JiraPocTicket[] = [];
  errorMessage = '';
  loading = false;
  listLoading = false;

  ngOnInit(): void {
    this.loadProjects();
    this.loadTickets();
  }

  loadProjects(): void {
    this.projectsLoading = true;
    this.jiraPocService.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.projectsLoading = false;
        if (projects.length > 0) {
          this.selectedProjectKey = projects[0].key;
          this.onProjectChange();
        }
      },
      error: () => {
        this.projectsLoading = false;
      }
    });
  }

  onProjectChange(): void {
    this.sprints = [];
    this.selectedSprintId = null;
    if (!this.selectedProjectKey) return;

    this.sprintsLoading = true;
    this.jiraPocService.getSprints(this.selectedProjectKey).subscribe({
      next: (sprints) => {
        this.sprints = sprints;
        this.sprintsLoading = false;
      },
      error: () => {
        this.sprintsLoading = false;
      }
    });
  }

  createTicket(): void {
    this.errorMessage = '';
    this.loading = true;

    const body: CreateJiraIssueRequest = {
      summary: this.title.trim(),
      description: this.description.trim() || undefined,
      priority: this.priority,
      issueType: this.issueType,
      projectKey: this.selectedProjectKey || undefined,
      sprintId: this.selectedSprintId ?? undefined
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
    if (!this.lastTicket?.jiraKey) return;

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

  sprintLabel(sprint: JiraSprint): string {
    return `${sprint.name} (${sprint.state})`;
  }
}
