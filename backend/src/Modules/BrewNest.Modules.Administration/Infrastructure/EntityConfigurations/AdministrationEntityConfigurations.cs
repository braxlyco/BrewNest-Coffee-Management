using BrewNest.Modules.Administration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Administration.Infrastructure.EntityConfigurations;

internal static class AdministrationSeed
{
    internal static readonly DateTimeOffset SeededAt = new(2026, 9, 20, 0, 0, 0, TimeSpan.Zero);
    internal static readonly Guid CompanyId = Guid.Parse("20000000-0000-0000-0000-000000000001");
}

internal abstract class AuditedConfigurationMap<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditedConfiguration
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ModifiedBy).HasMaxLength(100);
        builder.Property(entity => entity.RowVersion).IsRowVersion();
    }
}

internal sealed class CompanySettingsConfiguration : AuditedConfigurationMap<CompanySettings>
{
    public override void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        base.Configure(builder);
        builder.ToTable("CompanySettings");
        builder.Property(entity => entity.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.TradingName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.TaxRegistrationNumber).HasMaxLength(100);
        builder.Property(entity => entity.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(entity => entity.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Address).HasMaxLength(500);
        builder.Property(entity => entity.Phone).HasMaxLength(50);
        builder.Property(entity => entity.Email).HasMaxLength(256);
        builder.HasData(new CompanySettings
        {
            Id = AdministrationSeed.CompanyId,
            LegalName = "BrewNest Cafe",
            TradingName = "BrewNest",
            CurrencyCode = "PKR",
            TimeZoneId = "Asia/Karachi",
            CreatedAt = AdministrationSeed.SeededAt,
            CreatedBy = "initial-migration"
        });
    }
}

internal sealed class BranchConfiguration : AuditedConfigurationMap<Branch>
{
    public override void Configure(EntityTypeBuilder<Branch> builder)
    {
        base.Configure(builder);
        builder.ToTable("Branches");
        builder.Property(entity => entity.Code).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Address).HasMaxLength(500);
        builder.Property(entity => entity.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.HasIndex(entity => entity.Code).IsUnique();
    }
}

internal sealed class FiscalYearConfiguration : AuditedConfigurationMap<FiscalYear>
{
    public override void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        base.Configure(builder);
        builder.ToTable("FiscalYears");
        builder.Property(entity => entity.Name).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(entity => entity.Name).IsUnique();
    }
}

internal sealed class DocumentNumberSequenceConfiguration : AuditedConfigurationMap<DocumentNumberSequence>
{
    public override void Configure(EntityTypeBuilder<DocumentNumberSequence> builder)
    {
        base.Configure(builder);
        builder.ToTable("DocumentNumberSequences");
        builder.Property(entity => entity.DocumentType).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Prefix).HasMaxLength(30);
        builder.Property(entity => entity.Suffix).HasMaxLength(30);
        builder.Property(entity => entity.ResetPolicy).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(entity => new { entity.DocumentType, entity.BranchId }).IsUnique();
        builder.HasOne(entity => entity.Branch).WithMany().HasForeignKey(entity => entity.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ApprovalPolicyConfiguration : AuditedConfigurationMap<ApprovalPolicy>
{
    public override void Configure(EntityTypeBuilder<ApprovalPolicy> builder)
    {
        base.Configure(builder);
        builder.ToTable("ApprovalPolicies");
        builder.Property(entity => entity.ProcessCode).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.MinimumAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.MaximumAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.ApprovalMode).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(entity => new { entity.ProcessCode, entity.MinimumAmount, entity.MaximumAmount }).IsUnique();
        builder.HasData(SeedPolicies());
    }

    private static ApprovalPolicy[] SeedPolicies() =>
    [
        Policy(1, "Discount", "Discount approval", null, null, ApprovalMode.All),
        Policy(2, "VoidBeforePayment", "Void before payment", null, null, ApprovalMode.All),
        Policy(3, "VoidAfterPayment", "Void after payment", null, null, ApprovalMode.All),
        Policy(4, "Refund", "Refund approval", null, null, ApprovalMode.All),
        Policy(5, "PurchaseRequisition", "Purchase requisition", null, null, ApprovalMode.All),
        Policy(6, "ReturnedFoodRestocking", "Returned-food restocking", null, null, ApprovalMode.All),
        Policy(7, "StockAdjustment", "Stock adjustment", null, null, ApprovalMode.All),
        Policy(8, "Wastage", "Wastage approval", null, null, ApprovalMode.Any),
        Policy(9, "PurchaseOrder", "PO up to PKR 25,000", 0m, 25000m, ApprovalMode.All),
        Policy(10, "PurchaseOrder", "PO PKR 25,001 to PKR 100,000", 25000.01m, 100000m, ApprovalMode.All),
        Policy(11, "PurchaseOrder", "PO above PKR 100,000", 100000.01m, null, ApprovalMode.All)
    ];

    private static ApprovalPolicy Policy(int suffix, string process, string name, decimal? minimum, decimal? maximum, ApprovalMode mode) => new()
    {
        Id = Guid.Parse($"21000000-0000-0000-0000-{suffix:D12}"),
        ProcessCode = process,
        Name = name,
        MinimumAmount = minimum,
        MaximumAmount = maximum,
        ApprovalMode = mode,
        IsActive = true,
        CreatedAt = AdministrationSeed.SeededAt,
        CreatedBy = "initial-migration"
    };
}

internal sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RequiredRole).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.GrantedBy).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.RevokedBy).HasMaxLength(100);
        builder.Property(entity => entity.RowVersion).IsRowVersion();
        builder.HasIndex(entity => new { entity.ApprovalPolicyId, entity.Sequence }).HasFilter("[RevokedAt] IS NULL").IsUnique();
        builder.HasOne(entity => entity.ApprovalPolicy).WithMany(policy => policy.Steps).HasForeignKey(entity => entity.ApprovalPolicyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(SeedSteps());
    }

    private static ApprovalStep[] SeedSteps()
    {
        var entries = new List<(int Policy, string[] Roles)>
        {
            (1, ["Manager"]), (2, ["Sales Manager"]), (3, ["Sales Manager", "Finance"]),
            (4, ["Sales Manager", "Finance"]), (5, ["Requesting Department Manager"]),
            (6, ["Store/QC Manager"]), (7, ["Store Manager"]),
            (8, ["Kitchen Manager", "Store Manager"]), (9, ["Purchasing Manager"]),
            (10, ["Purchasing Manager", "Finance Manager"]),
            (11, ["Purchasing Manager", "Finance Manager", "Owner/Admin"])
        };
        var stepId = 1;
        return entries.SelectMany(entry => entry.Roles.Select((role, index) => new ApprovalStep
        {
            Id = Guid.Parse($"22000000-0000-0000-0000-{stepId++:D12}"),
            ApprovalPolicyId = Guid.Parse($"21000000-0000-0000-0000-{entry.Policy:D12}"),
            Sequence = index + 1,
            RequiredRole = role,
            GrantedAt = AdministrationSeed.SeededAt,
            GrantedBy = "initial-migration"
        })).ToArray();
    }
}

internal sealed class AlertSettingConfiguration : AuditedConfigurationMap<AlertSetting>
{
    public override void Configure(EntityTypeBuilder<AlertSetting> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlertSettings");
        builder.Property(entity => entity.Code).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(entity => entity.Code).IsUnique();
        builder.HasData(
            Alert(1, "ExpiryFirst", "First expiry alert", 15, AlertSeverity.Information),
            Alert(2, "ExpiryEscalated", "Escalated expiry alert", 7, AlertSeverity.Escalated),
            Alert(3, "ExpiryCritical", "Critical expiry alert", 3, AlertSeverity.Critical));
    }

    private static AlertSetting Alert(int suffix, string code, string name, int days, AlertSeverity severity) => new()
    {
        Id = Guid.Parse($"23000000-0000-0000-0000-{suffix:D12}"),
        Code = code,
        Name = name,
        DaysBefore = days,
        Severity = severity,
        IsActive = true,
        CreatedAt = AdministrationSeed.SeededAt,
        CreatedBy = "initial-migration"
    };
}

internal sealed class ExpenseAllocationSettingConfiguration : AuditedConfigurationMap<ExpenseAllocationSetting>
{
    public override void Configure(EntityTypeBuilder<ExpenseAllocationSetting> builder)
    {
        base.Configure(builder);
        builder.ToTable("ExpenseAllocationSettings");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Method).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(entity => entity.FixedPercentage).HasPrecision(5, 2);
        builder.HasOne(entity => entity.Branch).WithMany().HasForeignKey(entity => entity.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AdministrationAuditLogConfiguration : IEntityTypeConfiguration<AdministrationAuditLog>
{
    public void Configure(EntityTypeBuilder<AdministrationAuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Action).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Actor).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Reason).HasMaxLength(500);
        builder.Property(entity => entity.BeforeJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.AfterJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(entity => new { entity.EntityType, entity.EntityId, entity.OccurredAt });
    }
}
