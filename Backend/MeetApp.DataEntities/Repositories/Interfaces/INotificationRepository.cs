using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;

namespace MeetApp.DataEntities.Repositiories.Interfaces;

public interface INotificationRepository{
    Task<Result<int>> AddNotification(NotificationMessageQueue message);
    Task<Result> RemoveLikeNotification(NotificationMessageQueue message);
    Task<Result> RemoveFriendRequestNotification(NotificationMessageQueue message);
    Task<Result<List<RealtimeNotificationMessage>>> GetUserNotifications(AppUser user);
    Task<Result> MarkAsSeenAllNotifications(AppUser user);
    Task<Result<int>> GetAllUnseenNotifications(AppUser user);
}