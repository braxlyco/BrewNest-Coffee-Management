namespace BrewNest.Modules.Identity.Application.DTOs;

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "BrewNest.Api";
    public string Audience { get; set; } = "BrewNest.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
