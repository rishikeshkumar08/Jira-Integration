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
}
