using API.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

public class UserStatusHub(IUserRepository userRepository) : Hub{
    private static ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        OnlineUsers[userId] = Context.ConnectionId;

        await NotifyFriendsAsync(userId, "UserOnline");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        OnlineUsers.TryRemove(userId, out _);

        await NotifyFriendsAsync(userId, "UserOffline");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetOnlineUsers(){
        var callerConnectionId = Context.ConnectionId;
        var currentUsername = Context.User?.Identity?.Name;
        var friendsUsernames = await userRepository.GetUserFriendsUsernames(currentUsername);

        var onlineUsersExceptCurrent = OnlineUsers
            .Where(x => x.Value != callerConnectionId)
            .Select(v => v.Key)
            .ToArray();

        var onlineFriends = onlineUsersExceptCurrent.Intersect(friendsUsernames);

        await Clients.Caller.SendAsync("ReceiveOnlineUsers", onlineFriends);
    }

    private async Task NotifyFriendsAsync(string userId, string method){
        var friendsUsernames = await userRepository.GetUserFriendsUsernames(userId);
        
        var onlineFriends = OnlineUsers.Where(x => friendsUsernames.Contains(x.Key))
                                       .Select(x => x.Value).ToList();

        foreach(var connectionId in onlineFriends){
            await Clients.Client(connectionId).SendAsync(method, userId);
        }
    }

}