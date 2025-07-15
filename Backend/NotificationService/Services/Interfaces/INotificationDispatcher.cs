using MeetApp.DataEntities.Common;

namespace Notification.Services.Interfaces;

public interface INotificationDispatcher{
    Task<Result> DispatchAsync(NotificationMessageQueue message);
}