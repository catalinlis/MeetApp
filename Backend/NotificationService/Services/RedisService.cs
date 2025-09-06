using Notification.Services.Interfaces;
using StackExchange.Redis;

namespace Notification.Services;

public class RedisService : IRedisService{
    private readonly IDatabase _redis;
    public RedisService(IConnectionMultiplexer redis){
        _redis = redis.GetDatabase();
    }

    private string GetConnectionIdKey(string username) => $"signalr:notification:connections:{username}";
    private const string ActiveUsersKey = "signalr:notification:users";

    public async Task AddUserConnectionAsync(string username, string connectionId){
        await _redis.SetAddAsync(GetConnectionIdKey(username), connectionId);
        await _redis.SetAddAsync(ActiveUsersKey, username);
    }
    public async Task RemoveUserConnectionAsync(string username, string connectionId){
        var key = GetConnectionIdKey(username);
        await _redis.SetRemoveAsync(key, connectionId);

        if((await _redis.SetLengthAsync(key)) == 0){
            await _redis.SetRemoveAsync(ActiveUsersKey, username);
        }

    }
    public async Task<List<string>> GetActiveUsersAsync(){
        var users = await _redis.SetMembersAsync(ActiveUsersKey);
        return users.Select(u => u.ToString()).ToList();
    }
    public async Task<bool> IsUserActive(string username){
        return await _redis.SetLengthAsync(GetConnectionIdKey(username)) > 0;
    }
    public async Task<string> GetConnectionId(string username){
        var connectionIds = await _redis.SetMembersAsync(GetConnectionIdKey(username));
        return connectionIds.FirstOrDefault().ToString() ?? string.Empty;
    }
}