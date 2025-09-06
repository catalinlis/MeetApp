using MeetApp.DataEntities.DTOs;

namespace API.Helpers;

public static class NotificationMessageFactory{

    public static NotificationMessageQueue FromFeedItem(string notificationType, FeedItem feedItem, int senderId) => 
        new NotificationMessageQueue
        {
            Type = notificationType,
            SenderUserId = senderId,
            ResourceId = feedItem.PostId,
            ResourceType = feedItem.Type,
            Timestamp = feedItem.CreatedAt
        };

    public static NotificationMessageQueue FromCustom(string notificationType, int senderId, int targetId,
                                                    int? resourceId, string? resourceType, DateTimeOffset timestamp,
                                                    Dictionary<string, string>? metadata) =>
        new NotificationMessageQueue
        {
            Type = notificationType,
            SenderUserId = senderId,
            TargetUserId = targetId,
            ResourceId = resourceId,
            ResourceType = resourceType,
            Timestamp = timestamp,
            Metadata = metadata
        };

}