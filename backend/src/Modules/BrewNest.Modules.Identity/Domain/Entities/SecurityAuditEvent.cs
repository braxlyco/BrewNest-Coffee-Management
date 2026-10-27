using BrewNest.Modules.Identity.Domain.Enums;

namespace BrewNest.Modules.Identity.Domain.Entities;

internal sealed class SecurityAuditEvent
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public SecurityEventType EventType { get; set; }
    public bool Succeeded { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Details { get; set; } = string.Empty;
}
