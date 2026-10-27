using BrewNest.Contracts.Returns;using BrewNest.Modules.Administration.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Administration.Infrastructure.Integrations;
internal sealed class AdministrationReturnsApprovalPolicyPort(AdministrationDbContext database):IReturnsApprovalPolicyPort
{public async Task<ReturnsApprovalRoute?> ResolveAsync(string process,CancellationToken t){var x=await database.ApprovalPolicies.AsNoTracking().Include(p=>p.Steps).SingleOrDefaultAsync(p=>p.IsActive&&p.ProcessCode==process&&p.MinimumAmount==null&&p.MaximumAmount==null,t);return x is null?null:new(x.ProcessCode,x.Steps.Where(s=>s.RevokedAt==null).OrderBy(s=>s.Sequence).Select(s=>new ReturnsApprovalStep(s.Sequence,s.RequiredRole)).ToArray());}}
