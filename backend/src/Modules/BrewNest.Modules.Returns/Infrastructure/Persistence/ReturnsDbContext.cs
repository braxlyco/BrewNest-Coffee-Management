using BrewNest.Modules.Returns.Domain.Entities;using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Returns.Infrastructure.Persistence;
/// <summary>Owns Returns, RMA, credit-note, and void persistence.</summary>
public sealed class ReturnsDbContext(DbContextOptions<ReturnsDbContext> options):DbContext(options){internal DbSet<ReturnRequest> ReturnRequests=>Set<ReturnRequest>();internal DbSet<VoidRequest> VoidRequests=>Set<VoidRequest>();internal DbSet<CreditNote> CreditNotes=>Set<CreditNote>();internal DbSet<ReturnStatusHistory>StatusHistory=>Set<ReturnStatusHistory>();/// <inheritdoc />
protected override void OnModelCreating(ModelBuilder modelBuilder){modelBuilder.HasDefaultSchema("returns");modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReturnsDbContext).Assembly);base.OnModelCreating(modelBuilder);}}
