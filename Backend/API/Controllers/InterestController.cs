using API.Data;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class InterestController(DataContext context,
                                 UserManager<AppUser> userManager,
                                 IMapper mapper) : BaseController{
                        
    [HttpGet("{interest}")]
    public async Task<IActionResult> GetInterest(string interest){
        var interestData = await context.Interests.FirstOrDefaultAsync(x => x.InterestKey == interest);

        if(interestData == null)
            return BadRequest("There is no such interest");

        return Ok(mapper.Map<InterestDTO>(interestData));
    }

    [HttpGet("subscribed/{interestParam}/{usernameParam}")]
    public async Task<IActionResult> IsUserSubscribed(string interestParam, string usernameParam){
        var user = await context.Users.Include(i => i.Interests).FirstOrDefaultAsync(x => x.UserName == usernameParam);
        var interest = await context.Interests.FirstOrDefaultAsync(x => x.InterestKey == interestParam);

        if(user == null)
            return NotFound("User is not found");

        if(interest == null)
            return NotFound("Interest is not found");

        if(user.Interests.ToList().Contains(interest))
            return Ok(new {Subscribed = true});

        return Ok(new {Subscribed = false});
    }

    [HttpPost("{interestParam}/{usernameParam}")]
    public async Task<IActionResult> SubscribeInterest(string interestParam, string usernameParam){
        
        var user = await userManager.FindByNameAsync(usernameParam);
        var interest = await context.Interests.FirstOrDefaultAsync(x => x.InterestKey == interestParam);

        if(user == null)
            return NotFound("User is not found");

        if(interest == null)
            return NotFound("Interest is not found");

        user.Interests.Add(interest);
        await context.SaveChangesAsync();

        return Ok(new {Message = "Interest added"});
    }

}