namespace BrewNest.Contracts;

/// <summary>Represents the public health response returned by the BrewNest API.</summary>
public sealed record HealthStatusResponse(string Status, DateTimeOffset CheckedAt);
