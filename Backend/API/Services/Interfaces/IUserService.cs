using MeetApp.DataEntities.DTOs;

namespace API.Services.Interfaces;

public interface IUserService{
    Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames);
}