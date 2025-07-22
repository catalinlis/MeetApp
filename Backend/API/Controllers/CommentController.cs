using Microsoft.AspNetCore.Identity;
using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Data;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NotificationQueue.Services.Interfaces;
using API.Helpers;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.Repositiories.Interfaces;

namespace API.Controllers;

public class CommentController(
            UserManager<AppUser> userManager,
            ICommentService commentService,
            IQueueService queueService,
            IPhotoRepository photoRepository) : BaseController{
                
    [HttpGet("photo/{id}")]
    public async Task<IActionResult> GetPhotoComments(int id){
        
        if(await commentService.ExistPhotoId(id)){
            var comments = await commentService.GetPhotoComments(id);

            return Ok(new { Comments = comments });
        }

        return BadRequest("There is no such photo");
    }

    [HttpPost("photo/{id}")]
    public async Task<IActionResult> AddPhotoComment(int id, [FromForm] string username, [FromForm] string text){
        
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        if(await commentService.ExistPhotoId(id)){
            var comment = await commentService.AddPhotoComment(id, user, text);
            var targetUserId = await photoRepository.GetOwnerPhoto(id);

            if (targetUserId != -1)
            {
                var notificationMessage = NotificationMessageFactory.FromCustom(
                                    notificationType: NotificationTypes.Comment,
                                    senderId: user.Id,
                                    targetId: targetUserId,
                                    resourceId: id,
                                    resourceType: NotificationResourceType.Photo,
                                    timestamp: DateTimeOffset.UtcNow,
                                    metadata: null);
                await queueService.WriteMessage(notificationMessage);
            }

            return Ok(new { Comment = comment });
        }

        return BadRequest("There is no such photo");
    }

    [HttpGet("post/{id}")]
    public async Task<IActionResult> GetPostComments(int id){

        if(await commentService.ExistPostId(id)){
            var comments = await commentService.GetPostComments(id);

            return Ok(new { Comments = comments });
        }

        return BadRequest("There is no such photo");
    }

    [HttpPost("post/{id}")]
    public async Task<IActionResult> AddPostComment(int id, [FromForm] string username, [FromForm] string text){
        
        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        if(await commentService.ExistPostId(id)){
            var comment = await commentService.AddPostComment(id, user, text);

            var targetUserId = await photoRepository.GetOwnerPost(id);

            if (targetUserId != -1)
            {
                var notificationMessage = NotificationMessageFactory.FromCustom(
                                    notificationType: NotificationTypes.Comment,
                                    senderId: user.Id,
                                    targetId: targetUserId,
                                    resourceId: id,
                                    resourceType: NotificationResourceType.Post,
                                    timestamp: DateTimeOffset.UtcNow,
                                    metadata: null);

                await queueService.WriteMessage(notificationMessage);
            }

            return Ok(new { Comment = comment });
        }

        return BadRequest("There is no such post");
    }

}