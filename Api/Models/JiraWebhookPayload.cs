using System.Text.Json.Serialization;

namespace Api.Models;

public class JiraWebhookPayload
{
    [JsonPropertyName("webhookEvent")]
    public string? WebhookEvent { get; set; }

    [JsonPropertyName("issue")]
    public JiraWebhookIssue? Issue { get; set; }
}

public class JiraWebhookIssue
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("fields")]
    public JiraWebhookFields? Fields { get; set; }
}

public class JiraWebhookFields
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("description")]
    public object? Description { get; set; }

    [JsonPropertyName("priority")]
    public JiraNamedField? Priority { get; set; }

    [JsonPropertyName("status")]
    public JiraNamedField? Status { get; set; }
}

public class JiraNamedField
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}