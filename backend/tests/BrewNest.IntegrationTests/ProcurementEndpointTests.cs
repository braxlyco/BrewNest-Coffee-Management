using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
namespace BrewNest.IntegrationTests;
/// <summary>Exercises Procurement authorization and Problem Details behavior.</summary>
public sealed class ProcurementEndpointTests:IClassFixture<IdentityWebApplicationFactory>,IAsyncLifetime
{
 private readonly IdentityWebApplicationFactory factory;/// <summary>Creates the suite.</summary>
 public ProcurementEndpointTests(IdentityWebApplicationFactory factory)=>this.factory=factory;
 /// <inheritdoc />
 public async Task InitializeAsync(){await using var scope=factory.Services.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<ProcurementDbContext>().Database.EnsureCreatedAsync();}/// <inheritdoc />
 public Task DisposeAsync()=>Task.CompletedTask;
 /// <summary>Anonymous callers are rejected.</summary>
 [Fact]public async Task ProcurementRequiresAuthentication(){using var c=Client();using var r=await c.GetAsync("/api/procurement/requisitions");Assert.Equal(HttpStatusCode.Unauthorized,r.StatusCode);}
 /// <summary>Explicit permission is enforced.</summary>
 [Fact]public async Task UserWithoutProcurementPermissionIsForbidden(){using var c=Client();var admin=await Login(c,factory.AdministratorEmail,factory.AdministratorPassword);Bearer(c,admin.AccessToken);var email=$"procurement-denied-{Guid.NewGuid():N}@test.local";var password=IdentityWebApplicationFactory.CreateValidPassword();using var created=await c.PostAsJsonAsync("/api/identity/users",new CreateUserRequest(email,"Denied",password,[]));Assert.Equal(HttpStatusCode.OK,created.StatusCode);var token=await Login(c,email,password);Bearer(c,token.AccessToken);using var denied=await c.GetAsync("/api/procurement/requisitions");Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);}
 /// <summary>Validation failures use Problem Details.</summary>
 [Fact]public async Task InvalidRequisitionReturnsProblemDetails(){using var c=Client();var token=await Login(c,factory.AdministratorEmail,factory.AdministratorPassword);Bearer(c,token.AccessToken);using var r=await c.PostAsJsonAsync("/api/procurement/requisitions",new CreateRequisitionRequest("",DateOnly.FromDateTime(DateTime.UtcNow),"",[]));Assert.Equal(HttpStatusCode.BadRequest,r.StatusCode);Assert.Equal("application/problem+json",r.Content.Headers.ContentType?.MediaType);}
 private HttpClient Client()=>factory.CreateClient(new(){BaseAddress=new Uri("https://localhost")});private static void Bearer(HttpClient c,string token)=>c.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token);private static async Task<TokenResponse> Login(HttpClient c,string email,string password){c.DefaultRequestHeaders.Authorization=null;using var r=await c.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(email,password));Assert.Equal(HttpStatusCode.OK,r.StatusCode);return Assert.IsType<TokenResponse>(await r.Content.ReadFromJsonAsync<TokenResponse>());}
}
