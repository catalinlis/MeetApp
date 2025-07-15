using System.Security.Claims;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
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

    [HttpGet("photos/{username}")]
    public async Task<IActionResult> GetUserPhotos(string username){

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var photos = await context.Photos.Include(x => x.AddedBy)
                                         .Include(x => x.LikedByUsers)
                                         .Where(x => x.AddedById == user.Id).ToListAsync();

        var photosDTO = photos.Select(x => new FeedItem{
            Type = "Photo",
            Content = x.Text,
            ImageUrl = x.PhotoId,
            CreatedAt = x.AddedAt,
            Author = x.AddedBy.UserName,
            LikesCount = x.LikesCount,
            LikedByMe = x.LikedByUsers.Any(x => x.UserId == user.Id),
            CommentsCount = x.CommentsCount,
            PostId = x.Id
        }).OrderByDescending(x => x.CreatedAt);

        return Ok(new { photos = photosDTO });
    }

    [HttpGet("posts/{username}")]
    public async Task<IActionResult> GetUserPosts(string username){
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var currentUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var currentUser = await userManager.FindByNameAsync(currentUsername!);

        if(currentUser == null)
            return BadRequest("There is no such user");

        var posts = await context.Posts.Include(x => x.LikedByUsers)
                                       .Include(x => x.CreatedBy)
                                       .Where(x => x.CreatedBy.Id == user.Id)
                                       .ToListAsync();

        var postsFeed = new List<FeedItem>();
        
        foreach(var post in posts)
        {
            var hasLiked = post.LikedByUsers.Any(x => x.UserId == currentUser.Id);
            var postInterests = await context.PostInterests
                                                .Where(x => x.PostId == post.Id)
                                                .Select(x => x.Interest)
                                                .ToListAsync();

            var interests = mapper.Map<List<InterestDTO>>(postInterests);

            var postFeed = new FeedItem
            {
                Type = "Post",
                Content = post.Text,
                Author = post.CreatedBy.UserName,
                ImageUrl = post.ImageId,
                CreatedAt = post.CreatedAt,
                LikesCount = post.LikesCount,
                LikedByMe = hasLiked,
                CommentsCount = post.CommentsCount,
                PostId = post.Id,
                Interests = interests
            };

            postsFeed.Add(postFeed);

        } 

        var photos = await context.Photos.Include(x => x.LikedByUsers)
                                         .Include(x => x.AddedBy)
                                         .Where(x => x.AddedBy.Id == user.Id)
                                         .ToListAsync();

        var photosFeed = new List<FeedItem>();

        foreach(var photo in photos){

            var hasLiked = photo.LikedByUsers.Any(x => x.UserId == currentUser.Id);

            var photoFeed = new FeedItem
            {
                Type = "Photo",
                Content = photo.Text,
                Author = photo.AddedBy.UserName,
                ImageUrl = photo.PhotoId,
                CreatedAt = photo.AddedAt,
                LikesCount = photo.LikesCount,
                CommentsCount = photo.CommentsCount,
                LikedByMe = hasLiked,
                PostId = photo.Id
            };

            photosFeed.Add(photoFeed);

        }

        var feedItems = postsFeed.Concat(photosFeed).OrderByDescending(x => x.CreatedAt).ToList();

        return Ok(feedItems);
    }

}