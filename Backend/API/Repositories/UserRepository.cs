using API.Data;
using API.DTOs;
using API.Repositories.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Repositories;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames)
    {
        var users = await context.Users.Where(u => usernames.Contains(u.UserName)).ToListAsync();
        
        return users.AsQueryable().ProjectTo<UserMember>(mapper.ConfigurationProvider).ToList();
    }

    public async Task<List<string>> GetUserFriendsUsernames(string username){
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == username);
        var usernames = new List<string>();

        if(user !=  null){
            var friends = await context.Friendships.Where(x => x.UserId == user.Id).Select(f => f.Friend.UserName).ToListAsync();
            if(friends.Count > 0)
                usernames = friends!;
        }
        
        return usernames;
    }
}