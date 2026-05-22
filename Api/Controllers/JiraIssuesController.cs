using System.Text.Json;
using Api.Masters;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/jira/issues")]
public class JiraIssuesController : ControllerBase
{
    private readonly JiraRestClient _jira;
    private readonly JiraPocMaster _master;

    public JiraIssuesController(JiraRestClient jira, JiraPocMaster master)
    {
        _jira = jira;
        _master = master;
    }

    [HttpPost]
    public async Task<ActionResult<JiraPocTicket>> Create([FromBody] CreateJiraIssueRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Summary))
            return BadRequest("Summary is required.");

        var created = await _jira.CreateIssueAsync(request, ct);

        var ticket = _master.Upsert(
            jiraKey: created.Key,
            title: request.Summary,
            description: request.Description,
            priority: request.Priority,
            status: "To Do");

        return Ok(ticket);
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<JiraPocTicket>> List()
    {
        return Ok(_master.ListAll());
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<JiraPocTicket>> GetByKey(string key, CancellationToken ct)
    {
        var fromDb = _master.GetByJiraKey(key);
        if (fromDb != null)
            return Ok(fromDb);

        var jiraDoc = await _jira.GetIssueAsync(key, ct);
        if (jiraDoc == null)
            return NotFound();

        var root = jiraDoc.RootElement;
        var fields = root.GetProperty("fields");

        var summary = fields.TryGetProperty("summary", out var s)
            ? s.GetString() ?? key
            : key;

        var status = "To Do";
        if (fields.TryGetProperty("status", out var st) &&
            st.TryGetProperty("name", out var stName))
            status = stName.GetString() ?? status;

        var priority = "Medium";
        if (fields.TryGetProperty("priority", out var pr) &&
            pr.TryGetProperty("name", out var prName))
            priority = prName.GetString() ?? priority;

        string? description = null;
        if (fields.TryGetProperty("description", out var desc) &&
            desc.ValueKind == JsonValueKind.String)
            description = desc.GetString();

        var ticket = _master.Upsert(key, summary, description, priority, status);
        return Ok(ticket);
    }
}