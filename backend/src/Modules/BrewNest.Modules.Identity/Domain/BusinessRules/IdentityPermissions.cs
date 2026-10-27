namespace BrewNest.Modules.Identity.Domain.BusinessRules;

internal static class IdentityPermissions
{
    public const string UsersRead = "identity.users.read";
    public const string UsersManage = "identity.users.manage";
    public const string RolesRead = "identity.roles.read";
    public const string RolesManage = "identity.roles.manage";
    public const string PermissionsRead = "identity.permissions.read";
    public const string PermissionsManage = "identity.permissions.manage";
    public const string AuditRead = "identity.audit.read";

    public static readonly IReadOnlyDictionary<string, (string Description, string Group)> Definitions =
        new Dictionary<string, (string Description, string Group)>(StringComparer.Ordinal)
        {
            [UsersRead] = ("View users and their active roles.", "Users"),
            [UsersManage] = ("Create, update, disable, and reset users.", "Users"),
            [RolesRead] = ("View roles and their permission grants.", "Roles"),
            [RolesManage] = ("Create and update authorization roles.", "Roles"),
            [PermissionsRead] = ("View the permission catalog.", "Permissions"),
            [PermissionsManage] = ("Grant and revoke role permissions.", "Permissions"),
            [AuditRead] = ("View Identity security audit history.", "Audit")
        };
}
