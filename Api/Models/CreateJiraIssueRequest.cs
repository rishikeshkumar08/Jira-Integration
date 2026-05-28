using System.Text.Json.Serialization;

namespace Api.Models;

public class CreateJiraIssueRequest
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonPropertyName("issueType")]
    public string? IssueType { get; set; }

    [JsonPropertyName("projectKey")]
    public string? ProjectKey { get; set; }

    [JsonPropertyName("sprintId")]
    public int? SprintId { get; set; }

    // Custom fields — required by Jira for Task and Bug work types
    [JsonPropertyName("acceptanceCriteria")]
    public string? AcceptanceCriteria { get; set; }

    [JsonPropertyName("riceScore")]
    public int? RiceScore { get; set; }

    [JsonPropertyName("kanoClarification")]
    public string? KanoClarification { get; set; }
}
