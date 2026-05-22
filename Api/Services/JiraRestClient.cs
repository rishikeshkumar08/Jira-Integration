using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Api.Models;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class JiraRestClient
{
    private readonly HttpClient _http;
    private readonly JiraSettings _settings;

    public JiraRestClient(HttpClient http, IOptions<JiraSettings> options)
    {
        _http = http;
        _settings = options.Value;

        var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
        _http.BaseAddress = new Uri(baseUrl);

        var auth = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_settings.UserEmail}:{_settings.ApiToken}"));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<JiraIssueCreatedResponse> CreateIssueAsync(CreateJiraIssueRequest request, CancellationToken ct = default)
    {
        var issueType = string.IsNullOrWhiteSpace(request.IssueType)
            ? _settings.DefaultIssueType
            : request.IssueType;

        var fields = new Dictionary<string, object>
        {
            ["project"] = new { key = _settings.ProjectKey },
            ["summary"] = request.Summary,
            ["issuetype"] = new { name = issueType }
        };

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            fields["description"] = new
            {
                type = "doc",
                version = 1,
                content = new[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new[]
                        {
                            new { type = "text", text = request.Description }
                        }
                    }
                }
            };
        }

        var body = new { fields };
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("rest/api/3/issue", content, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Jira create failed ({(int)response.StatusCode}): {responseText}");

        var created = JsonSerializer.Deserialize<JiraIssueCreatedResponse>(
            responseText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (created == null || string.IsNullOrWhiteSpace(created.Key))
            throw new InvalidOperationException("Jira returned an empty create response.");

        return created;
    }

    public async Task<JsonDocument?> GetIssueAsync(string issueKey, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"rest/api/3/issue/{issueKey}", ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Jira get failed ({(int)response.StatusCode}): {responseText}");

        return JsonDocument.Parse(responseText);
    }
}