using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.ManyToMany;
using API.Services.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UserController(UserManager<AppUser> userManager,
                             DataContext context,
                             IMapper mapper) : BaseController{

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers(){
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var users = await context.Users.Include(x => x.Interests)
                            .Where(u => u.UserName != username)
                            .ProjectTo<UserMember>(mapper.ConfigurationProvider)
                            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("member/{username}")]
    public async Task<IActionResult> GetMember(string username){
        var user = await context.Users.Include(x => x.Interests).FirstOrDefaultAsync(x => x.UserName == username);

        if(user == null)
            return BadRequest("No such user");

        var userMember = mapper.Map<UserMember>(user);

        return Ok(userMember);
    }

    [HttpGet("member/about/{username}")]
    public async Task<IActionResult> GetMemberAbout(string username){
        
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        user = context.Users.FirstOrDefault(x => x.UserName == username);

        var aboutMember = mapper.Map<AboutMember>(user);

        return Ok(aboutMember);   
    }

    [HttpGet("member/interests/{username}")]
    public async Task<IActionResult> GetMemberInterests(string username){

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var interests = await context.Users.Include(x => x.Interests).FirstOrDefaultAsync(x => x.UserName == username);

        var interestsDto = interests!.UserInterests.Select(i => i.Interest).AsQueryable().ProjectTo<InterestDTO>(mapper.ConfigurationProvider).ToList();

        return Ok(interestsDto);
    }

    [HttpGet("online-users")]
    public async Task<IActionResult> GetUsersByUsernames([FromQuery] string[] usernames, [FromServices] IUserService userService){

        var users = await userService.GetUsersByUsernameAsync(usernames);

        return Ok(users);
    
    }

}