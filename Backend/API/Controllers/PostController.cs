using System.Security.Claims;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Entities.ManyToMany;
using API.Services;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using API.Helpers;
using NotificationQueue.Services.Interfaces;

namespace API.Controllers;

public class PostController(UserManager<AppUser> userManager,
                            DataContext context,
                            IPostService postService,
                            CloudFrontService cloudFront,
                            IQueueService queue) : BaseController{

    [HttpPost("{username}")]
    public async Task<IActionResult> AddPost([FromForm] IFormFile? file, [FromForm] string text, [FromForm] List<string> interestKeys, string username){

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("The user is not found");

        var result = await postService.AddPost(user, file, text, interestKeys);

        if(result.Success){
            var notificationMessage = NotificationMessageFactory.FromFeedItem(NotificationTypes.AddFeedItem, result.Data, user.Id);
            await queue.WriteMessage(notificationMessage);
            return Ok(new { feedItem = result.Data });
        }
        else
            return BadRequest(result.ErrorMessage);

    }

    [HttpPost("photo/{username}")]
    public async Task<IActionResult> AddPhoto([FromForm] IFormFile file, [FromForm] string text, string username){
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("The user is not found");

        if(file == null || file.Length <= 0)
            return BadRequest("No photo provided");

        var result = await postService.AddPhoto(user, file, text);
        
        if(result.Success){
            var notificationMessage = NotificationMessageFactory.FromFeedItem(NotificationTypes.AddFeedItem, result.Data, user.Id);
            await queue.WriteMessage(notificationMessage);

            return Ok(new { feedItem = result.Data });
        }
        else
            return BadRequest(result.ErrorMessage);

    }


    [HttpGet("{username}")]
    public async Task<IActionResult> GetPosts(string username){
        
        var user = await userManager.FindByNameAsync(username);
    
        if(user == null)
            return BadRequest("There is no such user");

        var result = await postService.GetFeed(user);

        if(result.Success)
            return Ok(result.Data.Take(5));
        else
            return BadRequest(result.ErrorMessage);
    }

    [HttpGet("photo/{imageId}")]
    public async Task<IActionResult> GetPhoto(string imageId){
        var filename = imageId;
        var path = "photos";
        string signedUrl = cloudFront.SignUrl(filename, path);

        await Task.CompletedTask;

        return Ok(new { signedUrl = signedUrl });

    }

    [HttpGet("post/{imageId}")]
    public async Task<IActionResult> GetPostPhoto(string imageId){
        var filename = imageId;
        var path = "posts";
        string signedUrl = cloudFront.SignUrl(filename, path);

        await Task.CompletedTask;

        return Ok(new { signedUrl = signedUrl });
    }

    [HttpPut("like/photo/{id}")]
    public async Task<IActionResult> PhotoLike(int id){
        var username = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if(username == null)
            return BadRequest("No username provided");

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var photo = await context.Photos.Include(x => x.LikedByUsers).FirstOrDefaultAsync(x => x.Id == id);

        if(photo == null)
            return BadRequest("There is no such photo");

        var result = await postService.LikePhoto(user, photo);

        if(result.Success){
            var type = result.Data ? NotificationTypes.Like : NotificationTypes.Unlike;
            var notificationMessage = NotificationMessageFactory.FromCustom(
                                    notificationType: type,
                                    senderId: user.Id,
                                    targetId: photo.AddedById,
                                    resourceId: photo.Id,
                                    resourceType: NotificationResourceType.Photo,
                                    timestamp: DateTimeOffset.UtcNow,
                                    metadata: null);
            await queue.WriteMessage(notificationMessage);
            

            return Ok(new { HasLiked = result.Data });
        }
        else
            return BadRequest(result.ErrorMessage);

    }

    [HttpPut("like/post/{id}")]
    public async Task<IActionResult> PostLike(int id){
        var username = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if(username == null)
            return BadRequest("No username provided");

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var post = await context.Posts.Include(x => x.LikedByUsers).FirstOrDefaultAsync(x => x.Id == id);

        if(post == null)
            return BadRequest("There is no such post");

        var result = await postService.LikePost(user, post);

        if(result.Success){
            var type = result.Data ? NotificationTypes.Like : NotificationTypes.Unlike;
            var notificationMessage = NotificationMessageFactory.FromCustom(
                                    notificationType: type,
                                    senderId: user.Id,
                                    targetId: post.CreatedById,
                                    resourceId: post.Id,
                                    resourceType: NotificationResourceType.Post,
                                    timestamp: DateTimeOffset.UtcNow,
                                    metadata: null);
            await queue.WriteMessage(notificationMessage);

            return Ok(new { HasLiked = result.Data });
        }
        else
            return BadRequest(result.ErrorMessage);
    }
 }