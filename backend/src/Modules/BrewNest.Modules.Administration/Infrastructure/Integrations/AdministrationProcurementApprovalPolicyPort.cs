using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Administration.Infrastructure.Integrations;
internal sealed class AdministrationProcurementApprovalPolicyPort(AdministrationDbContext database):IProcurementApprovalPolicyPort
{
    public async Task<ProcurementApprovalRoute?> ResolveAsync(string processCode,decimal? amount,CancellationToken token)
    {
        var policies=await database.ApprovalPolicies.AsNoTracking().Include(x=>x.Steps).Where(x=>x.IsActive&&x.ProcessCode==processCode).ToArrayAsync(token);
        var match=policies.Where(x=>amount is null||(x.MinimumAmount is null||amount>=x.MinimumAmount)&&(x.MaximumAmount is null||amount<=x.MaximumAmount)).OrderByDescending(x=>x.MinimumAmount??0).FirstOrDefault();
        return match is null?null:new(match.Id,match.Name,match.ApprovalMode.ToString(),match.Steps.Where(x=>x.RevokedAt==null).OrderBy(x=>x.Sequence).Select(x=>new ProcurementApprovalStep(x.Sequence,x.RequiredRole)).ToArray());
    }
}
