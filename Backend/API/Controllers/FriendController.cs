using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.ManyToMany;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class FriendController(DataContext context, UserManager<AppUser> userManager, IMapper mapper): BaseController{

[HttpPost("request/{sender}/{receiver}")]
    public async Task<IActionResult> SendFriendRequest(string sender, string receiver){
        var senderUser = await userManager.FindByNameAsync(sender);
        var receiverUser = await userManager.FindByNameAsync(receiver);

        if(senderUser == null)
            return BadRequest("There is no such user");
        
        if(receiverUser == null)
            return BadRequest("There is no such user");

        var friendReq = new FriendRequest{
            SentUserId = senderUser.Id,
            SentUser = senderUser,
            ReceiverUserId = receiverUser.Id,
            ReceivedUser = receiverUser,
            SentAt = DateTimeOffset.UtcNow,
            Accepted = false
        };

        context.FriendRequests.Add(friendReq);
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{current_user}/{target_user}")]
    public async Task<IActionResult> AreFriends(string current_user, string target_user){
        var currentUser = await userManager.FindByNameAsync(current_user);
        var targetUser = await userManager.FindByNameAsync(target_user);
        
        if(currentUser == null || targetUser == null)
            return BadRequest("There is no such user");

        var areFriendsRecord = await context.FriendRequests.FirstOrDefaultAsync(
            x => (x.SentUserId == currentUser.Id
                && x.ReceiverUserId == targetUser.Id
                && x.Accepted == true) ||
                ( x.SentUserId == targetUser.Id &&
                  x.ReceiverUserId == currentUser.Id &&
                  x.Accepted == true));

        bool areFriends = false;

        if(areFriendsRecord != null) areFriends = true;

        return Ok(new { AreFriends = areFriends });
    }

    [HttpGet("requests/{current_user}/{target_user}")]
    public async Task<IActionResult> FriendRequestSent(string current_user, string target_user){
        var currentUser = await userManager.FindByNameAsync(current_user);
        var targetUser = await userManager.FindByNameAsync(target_user);
    
        if(currentUser == null || targetUser == null)
            return BadRequest("There is no such user");

        var friendRequestRecord = await context.FriendRequests.FirstOrDefaultAsync(
            x => x.SentUserId == currentUser.Id
                && x.ReceiverUserId == targetUser.Id
                && x.Accepted == false);

        bool friendRequest = false;

        if(friendRequestRecord != null) friendRequest = true;

        return Ok(new { FriendRequest = friendRequest });
    }

    [HttpPost("requests/approve/{current_user}/{target_user}")]
    public async Task<IActionResult> ApproveFriendRequest(string current_user, string target_user){
        
        var currentUser = await userManager.FindByNameAsync(current_user);
        var targetUser = await userManager.FindByNameAsync(target_user);

        if(currentUser == null || targetUser == null)
            return BadRequest("There is no such user");

        var friendRequest = await context.FriendRequests.FirstOrDefaultAsync(x => 
                                x.SentUserId == targetUser.Id && 
                                x.ReceiverUserId == currentUser.Id &&
                                x.Accepted == false);
        
        if(friendRequest == null)
            return BadRequest("There is no such request");

        var friendshipCurrentUser = new Friendship{
            UserId = currentUser.Id,
            FriendId = targetUser.Id,
            FriendsSince = DateTimeOffset.UtcNow
        };

        var friendshipTargetUser = new Friendship{
            UserId = targetUser.Id,
            FriendId = currentUser.Id,
            FriendsSince = DateTimeOffset.UtcNow
        };

        friendRequest.Accepted = true;
        context.FriendRequests.Update(friendRequest);

        context.Friendships.Add(friendshipCurrentUser);
        context.Friendships.Add(friendshipTargetUser);
        
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetFriends(string username){
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var friendsQuery = await context.Friendships.Where(x => x.UserId == user.Id)
                                .Select(x => x.Friend).ToListAsync();

        if(friendsQuery == null)
            return Ok(new { Count = 0 });

        var friends = mapper.Map<List<UserMember>>(friendsQuery);

        return Ok(new { friends.Count, Friends = friends });

    }

}