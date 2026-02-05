using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

static string? GetArg(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    }
    return null;
}

var to = GetArg(args, "--to");
if (string.IsNullOrWhiteSpace(to))
{
    Console.WriteLine("Usage: --to you@example.com [--count 10] [--subject Test] [--html <p>Test</p>]");
    Environment.Exit(1);
}

var count = int.TryParse(GetArg(args, "--count"), out var c) ? c : 1;
if (count <= 0) count = 1;

var subject = GetArg(args, "--subject") ?? "Smoke";
var htmlBody = GetArg(args, "--html") ?? "<p>Test</p>";
var fromEmail = GetArg(args, "--fromEmail");
var fromName = GetArg(args, "--fromName");

var httpHost = GetArg(args, "--host") ?? Environment.GetEnvironmentVariable("RABBIT_HTTP_HOST") ?? "localhost";
var httpPort = GetArg(args, "--port") ?? Environment.GetEnvironmentVariable("RABBIT_HTTP_PORT") ?? "15672";
var httpUser = GetArg(args, "--user") ?? Environment.GetEnvironmentVariable("RABBIT_HTTP_USER") ?? "guest";
var httpPass = GetArg(args, "--pass") ?? Environment.GetEnvironmentVariable("RABBIT_HTTP_PASS") ?? "guest";

var exchange = GetArg(args, "--exchange") ?? Environment.GetEnvironmentVariable("RABBIT_EXCHANGE") ?? "mail.send";
var routingKey = GetArg(args, "--routing") ?? Environment.GetEnvironmentVariable("RABBIT_ROUTING") ?? "send";
var vhost = GetArg(args, "--vhost") ?? Environment.GetEnvironmentVariable("RABBIT_VHOST") ?? "/";

var baseUrl = $"http://{httpHost}:{httpPort}";
var publishUrl = $"{baseUrl}/api/exchanges/{Uri.EscapeDataString(vhost)}/{Uri.EscapeDataString(exchange)}/publish";

using var http = new HttpClient();
var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{httpUser}:{httpPass}"));
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

for (var i = 0; i < count; i++)
{
    var msg = new EmailSendRequested(
        Guid.NewGuid(),
        to!,
        count > 1 ? $"{subject} #{i + 1}" : subject,
        htmlBody,
        fromEmail,
        fromName);

    var payload = JsonSerializer.Serialize(msg);
    var body = new
    {
        properties = new { },
        routing_key = routingKey,
        payload,
        payload_encoding = "string",
    };

    using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    var resp = await http.PostAsync(publishUrl, content);
    var text = await resp.Content.ReadAsStringAsync();
    if (!resp.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
        Console.WriteLine(text);
        Environment.Exit(2);
    }
    else
    {
        Console.WriteLine(text);
    }
}

public sealed record EmailSendRequested(
    Guid MessageId,
    string To,
    string Subject,
    string HtmlBody,
    string? FromEmail,
    string? FromName
);
