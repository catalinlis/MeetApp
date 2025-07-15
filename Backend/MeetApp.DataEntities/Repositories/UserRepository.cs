using AutoMapper;
using AutoMapper.QueryableExtensions;
using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Repositiories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DataEntities.Repositiories;

public class UserRepository : IUserRepository{
    private readonly DataContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, UserManager<AppUser> userManager, IMapper mapper){
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }
    public async Task<Result<AppUser>> GetUserById(int id){
        
        var user = await _context.Users.FindAsync(id);

        if(user == null)
            return Result<AppUser>.Failure("There is no such user");

        return Result<AppUser>.Success(user);
    }
    public async Task<Result<AppUser>> GetUserByUsername(string username){

        var user = await _userManager.FindByNameAsync(username);

        if(user == null)
            return Result<AppUser>.Failure("There is no such user");

        return Result<AppUser>.Success(user);
    }
    public async Task<Result<string>> GetUsernameById(int id){

        var user = await _context.Users.FindAsync(id);

        if(user == null)
            return Result<string>.Failure("There is no such user");

        return Result<string>.Success(user.UserName!);
    }

    public async Task<Result<string>> GetFristnameById(int id){

        var user = await _context.Users.FindAsync(id);

        if(user == null)
            return Result<string>.Failure("There is no such user");

        return Result<string>.Success(user.Firstname);
    }

    public async Task<Result<string>> GetLastnameById(int id){

        var user = await _context.Users.FindAsync(id);

        if(user == null)
            return Result<string>.Failure("There is no such user");

        return Result<string>.Success(user.Lastname);
    }

    public async Task<Result<string>> GetProfilePhotoById(int id){

        var user = await _context.Users.FindAsync(id);
        
        if(user == null)
            return Result<string>.Failure("There is no such user");

        return Result<string>.Success(user.ProfilePhoto);
    }

    public async Task<IEnumerable<UserMember>> GetUsersByUsernameAsync(string[] usernames)
    {
        var users = await _context.Users.Where(u => usernames.Contains(u.UserName)).ToListAsync();
        
        return users.AsQueryable().ProjectTo<UserMember>(_mapper.ConfigurationProvider).ToList();
    }

    public async Task<List<string>> GetUserFriendsUsernames(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
        var usernames = new List<string>();

        if (user != null)
        {
            var friends = await _context.Friendships.Where(x => x.UserId == user.Id).Select(f => f.Friend.UserName).ToListAsync();
            if (friends.Count > 0)
                usernames = friends!;
        }

        return usernames;
    }
}