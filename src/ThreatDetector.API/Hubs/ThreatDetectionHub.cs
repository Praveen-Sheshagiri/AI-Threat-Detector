using Microsoft.AspNetCore.SignalR;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Hubs;

public class ThreatDetectionHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", Context.ConnectionId);
    }

    public async Task NotifyThreatDetected(ThreatEvent threat)
    {
        await Clients.All.SendAsync("ThreatDetected", threat);
    }

    public async Task NotifySecurityAlert(SecurityAlert alert)
    {
        await Clients.All.SendAsync("SecurityAlert", alert);
    }

    public async Task NotifyUserBehaviorAnomaly(UserBehaviorEvent behaviorEvent)
    {
        await Clients.All.SendAsync("UserBehaviorAnomaly", behaviorEvent);
    }

    public async Task NotifyZeroDayVulnerability(ZeroDayVulnerability vulnerability)
    {
        await Clients.All.SendAsync("ZeroDayVulnerability", vulnerability);
    }

    public async Task SendSystemStatus(object status)
    {
        await Clients.All.SendAsync("SystemStatus", status);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
} 