using System.Text.Json;
using Api.Masters;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/jira/webhook")]
[AllowAnonymous]
public class JiraWebhookController : ControllerBase
{
    private readonly JiraPocMaster _master;

    public JiraWebhookController(JiraPocMaster master)
    {
        _master = master;
    }

    [HttpPost]
    public IActionResult Receive([FromBody] JiraWebhookPayload payload)
    {
        if (payload?.Issue?.Key == null)
            return Ok(new { message = "Ignored: no issue key" });

        var issue = payload.Issue;
        var fields = issue.Fields;

        var title = fields?.Summary ?? issue.Key;
        var status = fields?.Status?.Name ?? "To Do";
        var priority = fields?.Priority?.Name ?? "Medium";

        var description = ExtractDescription(fields?.Description);

        _master.Upsert(issue.Key, title, description, priority, status);

        return Ok(new { message = "Webhook processed", key = issue.Key });
    }

    private static string? ExtractDescription(object? description)
    {
        if (description is string s)
            return s;

        if (description is JsonElement el)
            return ExtractAdfText(el);

        return null;
    }

    private static string? ExtractAdfText(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String)
            return el.GetString();

        if (el.ValueKind != JsonValueKind.Object)
            return null;

        if (!el.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return null;

        var parts = new List<string>();
        foreach (var node in content.EnumerateArray())
            CollectAdfText(node, parts);

        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    private static void CollectAdfText(JsonElement node, List<string> parts)
    {
        if (node.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            var value = text.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add(value);
        }

        if (node.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in content.EnumerateArray())
                CollectAdfText(child, parts);
        }
    }
}