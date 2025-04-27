using API.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

public class UserStatusRedisHub : Hub{

    private readonly IDatabase _redisDB;
    private readonly IUserRepository _userRepository;
    public UserStatusRedisHub(IConnectionMultiplexer redis, IUserRepository userRepository){
        _redisDB = redis.GetDatabase(); 
        _userRepository = userRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.Name;
        var connectionId = Context.ConnectionId;

        if(username != null){
            await _redisDB.StringSetAsync($"online:{username}", connectionId, TimeSpan.FromSeconds(120));
            await _redisDB.SetAddAsync("online_users", username);
            await NotifyFriendsAsync(username, "UserOnline");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context?.User?.Identity?.Name;

        if(username != null){
            await _redisDB.KeyDeleteAsync($"online:{username}");
            await _redisDB.SetRemoveAsync("online_users", username);
            await NotifyFriendsAsync(username, "UserOffline");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Heartbeat(){
        var username = Context?.User?.Identity?.Name;
        var connectionId = Context?.ConnectionId;

        if(username != null){
            await _redisDB.StringSetAsync($"online:{username}", connectionId, TimeSpan.FromSeconds(120));
        }
    }

    public async Task GetOnlineUsers(){
        var onlineUsers = await _redisDB.SetMembersAsync("online_users");
        var onlineUsersList = new List<string>();

        foreach( var user in onlineUsers){
            if( await _redisDB.KeyExistsAsync($"online:{user}"))
                if( user.ToString() != Context.User?.Identity?.Name )
                onlineUsersList.Add(user.ToString());
        }

        await Clients.Caller.SendAsync("ReceiveOnlineUsers", onlineUsersList);
    }

    private async Task NotifyFriendsAsync(string userId, string method){
        var friendsUsernames = await _userRepository.GetUserFriendsUsernames(userId);
        var onlineUsersRedis = await _redisDB.SetMembersAsync("online_users");
        var onlineUsers = onlineUsersRedis.Select( u => u.ToString()).ToList();
        
        var onlineFriends = onlineUsers.Intersect(friendsUsernames);

        foreach(var user in onlineFriends){
            Console.WriteLine(user);
            var connectionId = await _redisDB.StringGetAsync($"online:{user}");
            await Clients.Client(connectionId).SendAsync(method, userId);
        }
    }

}