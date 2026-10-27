namespace BrewNest.Modules.Identity.Application.DTOs;

internal sealed class IdentitySeedOptions
{
    public const string SectionName = "Identity:DevelopmentSeed";
    public bool Enabled { get; set; }
    public string AdministratorEmail { get; set; } = string.Empty;
    public string AdministratorPassword { get; set; } = string.Empty;
    public string AdministratorFullName { get; set; } = "BrewNest Administrator";
}
