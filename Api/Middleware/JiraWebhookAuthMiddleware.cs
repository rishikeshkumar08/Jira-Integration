using System.Security.Cryptography;
using System.Text;
using Api.Models;
using Microsoft.Extensions.Options;

namespace Api.Middleware;

public class JiraWebhookAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JiraWebhookAuthMiddleware> _logger;

    public JiraWebhookAuthMiddleware(RequestDelegate next, ILogger<JiraWebhookAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<JiraSettings> jiraOptions)
    {
        if (!IsWebhookRequest(context))
        {
            await _next(context);
            return;
        }

        var secret = jiraOptions.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogError("WebhookSecret is not configured.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("WebhookSecret is not configured.");
            return;
        }

        var signatureHeader = context.Request.Headers["X-Hub-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing X-Hub-Signature header.");
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (!IsValidSignature(body, secret, signatureHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid webhook signature.");
            return;
        }

        await _next(context);
    }

    private static bool IsWebhookRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api/jira/webhook", StringComparison.OrdinalIgnoreCase)
               && HttpMethods.IsPost(context.Request.Method);
    }

    private static bool IsValidSignature(string body, string secret, string signatureHeader)
    {
        var parts = signatureHeader.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !parts[0].Equals("sha256", StringComparison.OrdinalIgnoreCase))
            return false;

        var expectedHex = parts[1].ToLowerInvariant();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var calculated = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(calculated),
            Encoding.UTF8.GetBytes(expectedHex));
    }
}
