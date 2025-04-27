using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class ChatHub : Hub{

    private static ConcurrentDictionary<string, string> ActiveUsers = new ConcurrentDictionary<string, string>();

    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        ActiveUsers[userId] = Context.ConnectionId;

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        ActiveUsers.TryRemove(userId, out _);
        return base.OnDisconnectedAsync(exception);

    }

    public async Task SendPrivateMessageToUser(string sender, string receiver, string message){
        
        if(ActiveUsers.TryGetValue(receiver, out string? receiverConnection))
            await Clients.Client(receiverConnection!).SendAsync("ReceivePrivateMessage", receiver, sender, message);
    }

}