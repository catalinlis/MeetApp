using API.DTOs;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.UserServices;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames)
    {
        return await userRepository.GetUsersByUsernameAsync(usernames);
    }
}
