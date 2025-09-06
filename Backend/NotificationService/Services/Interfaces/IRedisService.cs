namespace Notification.Services.Interfaces;

public interface IRedisService{
    Task AddUserConnectionAsync(string username, string connectionId);
    Task RemoveUserConnectionAsync(string username, string connectionId);
    Task<List<string>> GetActiveUsersAsync();
    Task<bool> IsUserActive(string username);
    Task<string> GetConnectionId(string username);
}