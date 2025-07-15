using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;

namespace MeetApp.DataEntities.Repositiories.Interfaces;
public interface IUserRepository
{
    Task<Result<AppUser>> GetUserById(int id);
    Task<Result<AppUser>> GetUserByUsername(string username);
    Task<Result<string>> GetUsernameById(int id);
    Task<Result<string>> GetFristnameById(int id);
    Task<Result<string>> GetLastnameById(int id);
    Task<Result<string>> GetProfilePhotoById(int id);
    Task<List<string>> GetUserFriendsUsernames(string username); // TODO: return type! Result<List<string>>
    Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames); // TODO: return type! Result<IEnumerable<UserMember>>
}