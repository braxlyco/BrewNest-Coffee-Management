using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Payments.Domain.BusinessRules;

/// <summary>Explicit permissions owned by Payments.</summary>
public static class PaymentsPermissions
{
    /// <summary>View payments.</summary>
    public const string Read = "payments.read";
    /// <summary>Record payments.</summary>
    public const string Create = "payments.create";
    /// <summary>Verify payments.</summary>
    public const string Verify = "payments.verify";
    /// <summary>Settle verified payments.</summary>
    public const string Settle = "payments.settle";
    /// <summary>View refunds.</summary>
    public const string RefundsRead = "payments.refunds.read";
    /// <summary>Process refunds.</summary>
    public const string RefundsProcess = "payments.refunds.process";
}

internal sealed class PaymentsPermissionProvider : IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions { get; } =
    [
        new(PaymentsPermissions.Read, "View payments and settlement history.", "Payments"),
        new(PaymentsPermissions.Create, "Record customer payments.", "Payments"),
        new(PaymentsPermissions.Verify, "Verify or reject pending payments.", "Payments"),
        new(PaymentsPermissions.Settle, "Settle verified payments.", "Payments"),
        new(PaymentsPermissions.RefundsRead, "View refund requests.", "Payments"),
        new(PaymentsPermissions.RefundsProcess, "Process or reject refund requests.", "Payments")
    ];
}
