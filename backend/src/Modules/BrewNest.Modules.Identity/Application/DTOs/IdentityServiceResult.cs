namespace BrewNest.Modules.Identity.Application.DTOs;

internal enum IdentityResultStatus
{
    Success,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    ConcurrencyConflict
}

internal sealed record IdentityServiceResult<T>(IdentityResultStatus Status, T? Value, string[] Errors)
{
    public bool Succeeded => Status == IdentityResultStatus.Success;

    public static IdentityServiceResult<T> Success(T value) => new(IdentityResultStatus.Success, value, []);
    public static IdentityServiceResult<T> Failure(IdentityResultStatus status, params string[] errors) => new(status, default, errors);
}
