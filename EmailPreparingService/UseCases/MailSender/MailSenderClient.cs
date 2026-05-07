using System.Net.Http.Json;
using UseCases.SmtpProfile.CreateSmtpProfile;

namespace UseCases.MailSender;

public class MailSenderClient : IMailSenderClient
{
    private readonly HttpClient _http;

    public MailSenderClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<MailSenderSmtpProfile>> GetSmtpProfilesAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var response = await _http.GetAsync("/internal/smtp", cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Mail sender returned {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<MailSenderSmtpProfilesResponse>(cts.Token);
        return result?.smtpProfiles ?? [];
    }

    public async Task<Guid> CreateSmtpProfileAsync(CreateSmtpProfileRequest request)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    
        var response = await _http.PostAsJsonAsync("/internal/smtp", request, cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Mail sender returned {response.StatusCode}");
        }
    
        var result = await response.Content.ReadFromJsonAsync<Guid>(cts.Token);
        return result;
    }

    public async Task DeleteSmtpProfileAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    
        var response = await _http.DeleteAsync($"/internal/smtp/{id}", cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Mail sender returned {response.StatusCode} when deleting smtp {id}");
        }
    }
}