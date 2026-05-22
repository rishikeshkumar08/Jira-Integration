namespace Api.Models;

public class JiraSettings
{
    public string BaseUrl { get; set; } = "https://simplaiproduct-team.atlassian.net";
    public string UserEmail { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = "SCRUM";
    public string DefaultIssueType { get; set; } = "Task";
    public string? WebhookSecret { get; set; }
}