using API.DTOs;

namespace API.Repositories.Interfaces;

public interface IUserRepository{
    Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames);
    Task<List<string>> GetUserFriendsUsernames(string username);
}