using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Procurement.Application.Interfaces;
using BrewNest.Modules.Procurement.Application.Services;
using BrewNest.Modules.Procurement.Domain.BusinessRules;
using BrewNest.Modules.Procurement.Endpoints;
using BrewNest.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.Modules.Procurement.DependencyInjection;

/// <summary>Defines the Procurement module composition root.</summary>
/// <summary>Defines the Procurement module composition root.</summary>
public sealed class ProcurementModule():BrewNestModule("Procurement")
{
 /// <inheritdoc />
 public override void RegisterServices(IServiceCollection services,IConfiguration configuration){var connection=configuration.GetConnectionString("BrewNestDatabase")??string.Empty;services.TryAddSingleton(TimeProvider.System);services.AddDbContext<ProcurementDbContext>(o=>o.UseSqlServer(connection,s=>{s.MigrationsAssembly(typeof(ProcurementDbContext).Assembly.FullName);s.MigrationsHistoryTable("__EFMigrationsHistory","procurement");s.EnableRetryOnFailure();}));services.AddScoped<ProcurementService>();services.AddSingleton<IProcurementDocumentNumbers,ProcurementDocumentNumbers>();services.AddSingleton<IPermissionDefinitionProvider,ProcurementPermissionDefinitionProvider>();services.AddAuthorization(o=>{foreach(var permission in new[]{ProcurementPermissions.Read,ProcurementPermissions.RequisitionsManage,ProcurementPermissions.RequisitionsApprove,ProcurementPermissions.SourcingManage,ProcurementPermissions.OrdersManage,ProcurementPermissions.OrdersApprove,ProcurementPermissions.InspectionsManage,ProcurementPermissions.ReceiptsManage,ProcurementPermissions.ReturnsManage})o.AddPolicy(permission,p=>p.RequireClaim("permission",permission));});}
 /// <inheritdoc />
 public override void MapEndpoints(IEndpointRouteBuilder endpoints)=>endpoints.MapProcurementEndpoints();
}
