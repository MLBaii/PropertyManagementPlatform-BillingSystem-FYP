using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PropertyBill.Api.Services;

// Talks to the Expo Push API directly over HttpClient — a public endpoint, no auth needed
// for basic sending (Expo access tokens are an optional hardening step, out of scope here).
public class ExpoPushService : IExpoPushService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpoPushService> _logger;

    public ExpoPushService(HttpClient httpClient, ILogger<ExpoPushService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ExpoPushResult>> SendAsync(List<string> expoPushTokens, string title, string body, string? deepLink)
    {
        if (expoPushTokens.Count == 0)
        {
            return new List<ExpoPushResult>();
        }

        var messages = expoPushTokens.Select(token => new ExpoPushMessage
        {
            To = token,
            Title = title,
            Body = body,
            Sound = "default",
            Data = deepLink is null ? null : new Dictionary<string, string> { ["deepLink"] = deepLink },
        }).ToList();

        HttpResponseMessage response;
        try
        {
            // Confirmed live against the real Expo endpoint: it wants a bare JSON array of
            // messages at the request root (wrapping in {"messages": [...]} gets rejected
            // with "$: Expected array, received object; to: Required" — it tries to validate
            // the wrapper itself as a single message). The other real issue was a literal
            // `data: null` on a message with no deep link, fixed via [JsonIgnore] below.
            response = await _httpClient.PostAsJsonAsync("--/api/v2/push/send", messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Expo push request failed for {Count} token(s)", expoPushTokens.Count);
            return expoPushTokens.Select(t => new ExpoPushResult(t, false, "RequestFailed")).ToList();
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Expo push API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            return expoPushTokens.Select(t => new ExpoPushResult(t, false, "HttpError")).ToList();
        }

        var parsed = await response.Content.ReadFromJsonAsync<ExpoPushApiResponse>();
        if (parsed?.Data is null || parsed.Data.Count != expoPushTokens.Count)
        {
            _logger.LogError(
                "Expo push API returned an unexpected ticket count for {Count} token(s): got {TicketCount}",
                expoPushTokens.Count, parsed?.Data?.Count ?? 0);
            return expoPushTokens.Select(t => new ExpoPushResult(t, false, "UnexpectedResponse")).ToList();
        }

        // Expo returns tickets in the same array order as the request's messages.
        return expoPushTokens.Zip(parsed.Data, (token, ticket) =>
        {
            var success = string.Equals(ticket.Status, "ok", StringComparison.OrdinalIgnoreCase);
            if (!success)
            {
                _logger.LogWarning(
                    "Expo push ticket error for token {Token}: {Message} ({ErrorCode})",
                    token, ticket.Message, ticket.Details?.Error);
            }
            return new ExpoPushResult(token, success, ticket.Details?.Error);
        }).ToList();
    }

    private class ExpoPushMessage
    {
        public string To { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Sound { get; set; }

        // Expo also rejects a literal `null` here ("expected record, received null") — omit
        // the field entirely when there's no deep link rather than sending Data: null.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Data { get; set; }
    }

    private class ExpoPushApiResponse
    {
        public List<ExpoPushTicket>? Data { get; set; }
    }

    private class ExpoPushTicket
    {
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public ExpoPushTicketDetails? Details { get; set; }
    }

    private class ExpoPushTicketDetails
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
