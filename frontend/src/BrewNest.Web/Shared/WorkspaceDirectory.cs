namespace BrewNest.Web.Shared;

internal sealed record Workspace(string Name, string Slug, string Description, string Category, string Icon, bool Available = true)
{
    public string Url => Available ? Slug : $"module/{Slug}";
}

internal static class WorkspaceDirectory
{
    public static readonly Workspace[] All =
    [
        new("Sales", "sales", "Take orders, approve discounts and prepare the bill.", "Service", "receipt"),
        new("Kitchen", "kitchen", "Keep every ticket moving from preparation to ready.", "Service", "cup"),
        new("Catalog", "catalog", "Your menu, ingredients, recipes and café essentials.", "Operations", "book"),
        new("Inventory", "inventory", "Follow stock, reservations, batches and expiry dates.", "Operations", "box"),
        new("Production", "production", "Plan the prep, issue ingredients and record consumption.", "Operations", "layers"),
        new("Procurement", "procurement", "Manage suppliers, purchasing and incoming stock.", "Operations", "bag"),
        new("Returns", "returns", "Review returns, inspect items and manage credits.", "Service", "return"),
        new("Payments", "payments", "Verify receipts, reconcile payments and process refunds.", "Service", "wallet"),
        new("Administration", "administration", "Configure your café, approvals and operating rules.", "Management", "settings"),
        new("Identity & Access", "identity", "Manage your team, roles and account security.", "Management", "users"),
        new("Delivery", "delivery", "Rider assignment, delivery tracking and COD custody.", "Coming soon", "truck", false),
        new("Finance", "finance", "Expenses, allocations and café profitability.", "Coming soon", "chart", false),
        new("Reporting", "reporting", "Operational reports and business insights.", "Coming soon", "chart", false)
    ];
}
