using BrewNest.Modules.Administration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Administration.Infrastructure.Persistence;

/// <summary>Owns Administration configuration and audit persistence.</summary>
public sealed class AdministrationDbContext(DbContextOptions<AdministrationDbContext> options) : DbContext(options)
{
    internal DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    internal DbSet<Branch> Branches => Set<Branch>();
    internal DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    internal DbSet<DocumentNumberSequence> DocumentNumberSequences => Set<DocumentNumberSequence>();
    internal DbSet<ApprovalPolicy> ApprovalPolicies => Set<ApprovalPolicy>();
    internal DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    internal DbSet<AlertSetting> AlertSettings => Set<AlertSetting>();
    internal DbSet<ExpenseAllocationSetting> ExpenseAllocationSettings => Set<ExpenseAllocationSetting>();
    internal DbSet<AdministrationAuditLog> AuditLogs => Set<AdministrationAuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("administration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
