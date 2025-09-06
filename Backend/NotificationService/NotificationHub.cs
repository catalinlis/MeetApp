using MeetApp.DataEntities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Notification.Services.Interfaces;
using StackExchange.Redis;

[Authorize]
public class NotificationHub : Hub{

    private readonly IRedisService _redisService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IRedisService redisService, ILogger<NotificationHub> logger){
        _redisService = redisService;
        _logger = logger;
    }
    public override async Task OnConnectedAsync(){
        var username = Context?.User?.Identity?.Name;
        var connectionId = Context?.ConnectionId;

        if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(connectionId)){
            Console.WriteLine($"{username} connected: {connectionId}");
            await _redisService.AddUserConnectionAsync(username, connectionId); 
        }
        else{
            _logger.LogWarning($"Couldn't register (username, connectionId) => {username}:{connectionId}");
        }

        await base.OnConnectedAsync();

    }

    public override async Task OnDisconnectedAsync(Exception? ex){
        var username = Context?.User?.Identity?.Name;
        var connectionId = Context?.ConnectionId;

        if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(connectionId)){
            Console.WriteLine($"{username} disconnected");
            await _redisService.RemoveUserConnectionAsync(username, connectionId);
        }
        else{
            _logger.LogWarning($"Couldn't register (username, connectionId) => {username}:{connectionId}");
        }

        await base.OnDisconnectedAsync(ex);
    }
}