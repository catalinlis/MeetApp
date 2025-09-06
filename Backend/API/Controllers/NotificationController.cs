using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class NotificationController(IUserRepository userRepository,
                                    INotificationRepository notificationRepository): BaseController{
    
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUserNotifications(string username){
        var user = await userRepository.GetUserByUsername(username);

        if(user.IsError)
            return BadRequest(user.Error);

        var notifications = await notificationRepository.GetUserNotifications(user.Data!);

        if(notifications.IsError)
            return BadRequest(notifications.Error);

        return Ok(notifications.Data!.OrderByDescending(x => x.CreatedAt));
    }

    [HttpPut("seen/{username}")]
    public async Task<IActionResult> MarkAsSeenAllNotifications(string username){
        var user = await userRepository.GetUserByUsername(username);

        if(user.IsError)
            return BadRequest(user.Error);

        var markAsSeen = await notificationRepository.MarkAsSeenAllNotifications(user.Data!);

        if(markAsSeen.IsError)
            return BadRequest(markAsSeen.Error);

        return Ok();
    }

    [HttpGet("unseen/{username}")]
    public async Task<IActionResult> GetUnseenNotifications(string username){
        
        var user = await userRepository.GetUserByUsername(username);

        if(user.IsError)
            return BadRequest(user.Error);

        var unseenNotifications = await notificationRepository.GetAllUnseenNotifications(user.Data);

        if(unseenNotifications.IsError)
            return BadRequest(unseenNotifications.Error);

        return Ok(unseenNotifications.Data);
    }

}