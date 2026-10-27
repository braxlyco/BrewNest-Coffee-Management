using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Administration.Domain.BusinessRules;

internal static class AdministrationPermissions
{
    public const string SettingsRead = "administration.settings.read";
    public const string SettingsManage = "administration.settings.manage";
    public const string ApprovalsRead = "administration.approvals.read";
    public const string ApprovalsManage = "administration.approvals.manage";
    public const string AuditRead = "administration.audit.read";
}

internal sealed class AdministrationPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions { get; } =
    [
        new(AdministrationPermissions.SettingsRead, "View company, branch, fiscal, numbering, alert, and allocation settings.", "Administration"),
        new(AdministrationPermissions.SettingsManage, "Create and update operational configuration.", "Administration"),
        new(AdministrationPermissions.ApprovalsRead, "View configurable approval routes and value tiers.", "Approvals"),
        new(AdministrationPermissions.ApprovalsManage, "Create, update, activate, and deactivate approval routes.", "Approvals"),
        new(AdministrationPermissions.AuditRead, "View the Administration configuration audit history.", "Audit")
    ];
}
