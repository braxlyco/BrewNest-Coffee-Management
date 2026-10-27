namespace BrewNest.Modules.Identity.Domain.Enums;

internal enum SecurityEventType
{
    LoginSucceeded,
    LoginFailed,
    TokenRefreshed,
    TokenReplayDetected,
    TokenRevoked,
    PasswordChanged,
    PasswordReset,
    UserCreated,
    UserUpdated,
    UserRolesChanged,
    RoleCreated,
    RoleUpdated,
    RolePermissionsChanged
}
