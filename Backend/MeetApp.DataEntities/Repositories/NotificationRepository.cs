using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Repositiories.Interfaces;
using MeetApp.DataEntities.Utils;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DataEntities.Repositiories;

public class NotificationRepository: INotificationRepository{
    private readonly DataContext _context;

    public NotificationRepository(DataContext context){
        _context = context;
    }
    public async Task<Result<int>> AddNotification(NotificationMessageQueue message){
        var notificationMessage = new Notification
        {
            Type = message.Type,
            SenderUserId = message.SenderUserId,
            TargetUserId = message.TargetUserId,
            ResourceId = message.ResourceId,
            ResourceType = message.ResourceType,
            Timestamp = message.Timestamp,
            Metadata = message.Metadata
        };

        _context.Notifications.Add(notificationMessage);
        await _context.SaveChangesAsync();

        return Result<int>.Success(notificationMessage.Id);
    }
    public async Task<Result> RemoveLikeNotification(NotificationMessageQueue message){

        var notification = await _context.Notifications.
                                FirstOrDefaultAsync(x => x.Type == NotificationTypes.Like &&
                                                         x.SenderUserId == message.SenderUserId &&
                                                         x.TargetUserId == message.TargetUserId &&
                                                         x.ResourceId == message.ResourceId &&
                                                         x.ResourceType == message.ResourceType);

        if(notification != null){
            _context.Remove(notification);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        return Result.Failure("RemoveLike -> The notification was not found");
    }

    public async Task<Result> RemoveFriendRequestNotification(NotificationMessageQueue message){
        var notification = await _context.Notifications.
                                    FirstOrDefaultAsync(x => x.Type == NotificationTypes.FriendRequest &&
                                                             x.SenderUserId == message.TargetUserId &&
                                                             x.TargetUserId == message.SenderUserId);

        if(notification != null){
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        return Result.Failure("Remove Friend Request -> The notification was not found");
    }

    public async Task<Result<List<RealtimeNotificationMessage>>> GetUserNotifications(AppUser user){
        
        var notifications = await _context.Notifications.Include(x => x.SenderUser)
                                                        .Where(x => x.TargetUserId == user.Id)
                                                        .ToListAsync();

        var realtimeNotifications = new List<RealtimeNotificationMessage>();

        foreach(var notification in notifications){
            var message = NotificationUtils.GenerateNotificationMessage(notification.Type, notification.ResourceType);

            if (message.IsSuccess)
            {
                var realtimeNotification = new RealtimeNotificationMessage
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Message = message.Data,
                    SenderFirstname = notification.SenderUser.Firstname,
                    SenderLastname = notification.SenderUser.Lastname,
                    SenderProfilePhoto = notification.SenderUser.ProfilePhoto,
                    CreatedAt = notification.Timestamp
                };

                realtimeNotifications.Add(realtimeNotification);
            }
        }

        return Result<List<RealtimeNotificationMessage>>.Success(realtimeNotifications);
    }

    public async Task<Result> MarkAsSeenAllNotifications(AppUser user){

        var notifications = await _context.Notifications.Where(x => x.TargetUserId == user.Id)
                                                        .ToListAsync();

        notifications.ForEach(x => x.Seen = true);

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<int>> GetAllUnseenNotifications(AppUser user){

        var notifications = await _context.Notifications.Where(x => x.TargetUserId == user.Id && x.Seen == false)
                                                        .ToListAsync();

        return Result<int>.Success(notifications.Count);
    }

 }