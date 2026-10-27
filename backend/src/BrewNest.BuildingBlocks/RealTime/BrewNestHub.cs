using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BrewNest.BuildingBlocks.RealTime;

/// <summary>
/// Provides the authenticated real-time transport used by module-specific KOT and alert notifications.
/// </summary>
[Authorize]
public sealed class BrewNestHub : Hub;
