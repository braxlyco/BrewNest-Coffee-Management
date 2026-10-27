using System.Security.Claims;
using System.Text;
using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Identity.Application.DTOs;
using BrewNest.Modules.Identity.Application.Interfaces;
using BrewNest.Modules.Identity.Application.Services;
using BrewNest.Modules.Identity.Domain.BusinessRules;
using BrewNest.Modules.Identity.Domain.Entities;
using BrewNest.Modules.Identity.Endpoints;
using BrewNest.Modules.Identity.Infrastructure.Integrations;
using BrewNest.Modules.Identity.Infrastructure.Persistence;
using BrewNest.Modules.Identity.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BrewNest.Modules.Identity.DependencyInjection;

/// <summary>Defines the Identity module composition root.</summary>
public sealed class IdentityModule() : BrewNestModule("Identity")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<IdentitySeedOptions>(configuration.GetSection(IdentitySeedOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        var connectionString = configuration.GetConnectionString("BrewNestDatabase") ?? string.Empty;
        services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
                sqlOptions.EnableRetryOnFailure();
            }));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, configuredOptions) =>
            {
                var jwt = configuredOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = string.IsNullOrWhiteSpace(jwt.SigningKey)
                        ? null
                        : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in IdentityPermissions.Definitions.Keys)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
            }
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IPermissionDefinitionProvider, IdentityPermissionDefinitionProvider>();
        services.AddHostedService<IdentityDevelopmentSeeder>();
    }

    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapIdentityEndpoints();
}
