using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BrewNest.Web.Services;

internal static class ApiProblemMessage
{
    private static readonly Regex DatabaseId = new(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Proxies and authentication middleware can return empty or non-Problem-Details bodies.
    public static string Read(string content, HttpStatusCode statusCode, string feature)
    {
        var fallback = statusCode switch
        {
            HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
            HttpStatusCode.Forbidden => "Your account does not have permission for this action.",
            HttpStatusCode.BadRequest => $"Please check the {feature} information and try again.",
            HttpStatusCode.NotFound => "This record is no longer available. Refresh the page and try again.",
            HttpStatusCode.Conflict => "This action could not be completed with the current information. Refresh the page and try again.",
            _ => $"We couldn't complete the {feature} action right now. Please try again or ask a manager for help."
        };

        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) return fallback;

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("detail", out var detail) &&
                detail.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                var message = detail.GetString()!;
                if (!DatabaseId.IsMatch(message)) return message;
                if (message.Contains("stock", StringComparison.OrdinalIgnoreCase))
                    return "Not enough usable stock for a required ingredient. Check the ingredient's lots and reservations in Inventory, then try again.";
                return "A selected record is no longer available. Refresh the page, check your selection, and try again.";
            }
        }
        catch (JsonException)
        {
            // Keep raw HTML and exception payloads out of user-facing error messages.
        }

        return fallback;
    }
}
