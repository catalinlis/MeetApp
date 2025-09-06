using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Configurations;

namespace MeetApp.DataEntities.Utils;

public static class NotificationUtils{
    public static Result<string> GenerateNotificationMessage(string type, string resourceType){
        
        string message = "";
        
        if(type == NotificationTypes.Like && resourceType == NotificationResourceType.Photo)
            message = "liked your photo";
        else if(type == NotificationTypes.Like && resourceType == NotificationResourceType.Post)
            message = "liked your post";
        else if(type == NotificationTypes.Comment && resourceType == NotificationResourceType.Photo)
            message = "commented your photo";
        else if(type == NotificationTypes.Comment && resourceType == NotificationResourceType.Post)
            message = "commented your post";
        else if(type == NotificationTypes.AddFeedItem && resourceType == NotificationResourceType.Photo)
            message = "added new photo";
        else if(type == NotificationTypes.AddFeedItem && resourceType == NotificationResourceType.Post)
            message = "added new post";
        else if(type == NotificationTypes.FriendRequest)
            message = "sent you friend request";
        else if(type == NotificationTypes.FriendRequestAccepted)
            message = "accepted your friend request";

        if(message == "")
            return Result<string>.Failure("Message cannot be generated");
        else
            return Result<string>.Success(message);

    }
}