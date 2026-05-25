using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/jira")]
public class JiraProjectsController : ControllerBase
{
    private readonly JiraRestClient _jira;

    public JiraProjectsController(JiraRestClient jira)
    {
        _jira = jira;
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<JiraProjectDto>>> GetProjects(CancellationToken ct)
    {
        var projects = await _jira.GetProjectsAsync(ct);
        return Ok(projects);
    }

    [HttpGet("projects/{projectKey}/sprints")]
    public async Task<ActionResult<List<JiraSprintDto>>> GetSprints(string projectKey, CancellationToken ct)
    {
        var boardId = await _jira.GetBoardIdForProjectAsync(projectKey, ct);
        if (boardId == null)
            return Ok(new List<JiraSprintDto>());

        var sprints = await _jira.GetSprintsAsync(boardId.Value, ct);
        return Ok(sprints);
    }
}
