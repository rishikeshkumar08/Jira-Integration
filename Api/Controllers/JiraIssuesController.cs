using System.Text.Json;
using Api.Masters;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Controllers;

[ApiController]
[Route("api/jira/issues")]
public class JiraIssuesController : ControllerBase
{
    private readonly JiraRestClient _jira;
    private readonly JiraPocMaster _master;
    private readonly JiraSettings _jiraSettings;

    public JiraIssuesController(JiraRestClient jira, JiraPocMaster master, IOptions<JiraSettings> jiraOptions)
    {
        _jira = jira;
        _master = master;
        _jiraSettings = jiraOptions.Value;
    }

    [HttpPost]
    public async Task<ActionResult<JiraPocTicket>> Create([FromBody] CreateJiraIssueRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Summary))
            return BadRequest("Summary is required.");

        var effectiveIssueType = string.IsNullOrWhiteSpace(request.IssueType)
            ? _jiraSettings.DefaultIssueType
            : request.IssueType;

        var requiresCustomFields = effectiveIssueType.Equals("Task", StringComparison.OrdinalIgnoreCase)
                                || effectiveIssueType.Equals("Bug", StringComparison.OrdinalIgnoreCase);

        if (requiresCustomFields)
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AcceptanceCriteria))
                missing.Add("Acceptance Criteria");

            if (!request.RiceScore.HasValue)
                missing.Add("Rice Score");

            if (string.IsNullOrWhiteSpace(request.KanoClarification))
                missing.Add("Kano Clarification");

            if (missing.Count > 0)
                return UnprocessableEntity(
                    $"The following fields are required for {effectiveIssueType}: {string.Join(", ", missing)}.");
        }

        var created = await _jira.CreateIssueAsync(request, ct);

        if (request.SprintId.HasValue)
            await _jira.MoveIssueToSprintAsync(request.SprintId.Value, created.Key, ct);

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
