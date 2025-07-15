using System.Security.AccessControl;
using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Repositiories;
using MeetApp.DataEntities.Repositiories.Interfaces;
using MeetApp.DataEntities.Utils;
using Microsoft.AspNetCore.SignalR;
using Notification.Services.Interfaces;
using NotificationQueue.Helpers;

namespace Notification.Services;

public class NotificationDispatcher : INotificationDispatcher{

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private INotificationRepository _notificationRepository;
    private IUserRepository _userRepository;
    private readonly IRedisService _redisService;

    public NotificationDispatcher(IHubContext<NotificationHub> hubContext,
                                  ILogger<NotificationDispatcher> logger,
                                  IServiceScopeFactory scopeFactory,
                                  IRedisService redisService){
        _hubContext = hubContext;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _redisService = redisService;
    }

    public async Task<Result> DispatchAsync(NotificationMessageQueue message){

        using var scope = _scopeFactory.CreateScope();
        _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        Result<RealtimeNotificationMessage>? result = null;

        switch(message.Type){
            case NotificationTypes.Like:{
                result = await HandleLike(message);
                break;
            }
            case NotificationTypes.Unlike:{
                result = await HandleUnlike(message);
                break;
            }
            case NotificationTypes.Comment:{
                result = await HandleComment(message);
                break;
            }
            case NotificationTypes.AddFeedItem:{
                result = await HandleNewFeedItem(message);
                break;
            }
            case NotificationTypes.FriendRequest:{
                result = await HandleFriendRequest(message);
                break;
            }
            case NotificationTypes.FriendRequestAccepted:{
                result = await HandleFriendRequestAccepted(message);
                break;
            }
            default:{
                _logger.LogWarning($"Unknown message type: {message.Type}");
                break;
            }
        }

        if(result.IsError){
            return Result.Failure(result.Error);
        }
        else{
            if(message.Type != NotificationTypes.Unlike && result.Data != null){
                var username = await _userRepository.GetUsernameById(message.TargetUserId);
                
                if(username.IsError)
                    return Result.Failure(username.Error);

                if (message.TargetUserId != message.SenderUserId)
                {

                    var connectionId = await _redisService.GetConnectionId(username.Data);

                    ObjectPrinter.PrintProperties(result.Data);
                    Console.WriteLine(connectionId);

                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("notification", result.Data);
                    }
                }
            }

            return Result.Success();
        }
    }

    private async Task<Result<RealtimeNotificationMessage>> HandleLike(NotificationMessageQueue message){
        var result = await _notificationRepository.AddNotification(message);

        if(result.IsError)
            return Result<RealtimeNotificationMessage>.Failure(result.Error);

        var sendMessage = await MessagePacking(message, result.Data);

        if(sendMessage.IsError)
            return Result<RealtimeNotificationMessage>.Failure(sendMessage.Error);

        return Result<RealtimeNotificationMessage>.Success(sendMessage.Data);
    }
    private async Task<Result<RealtimeNotificationMessage>> HandleUnlike(NotificationMessageQueue message){
        var result = await _notificationRepository.RemoveLikeNotification(message);

        if(result.IsError)
            return Result<RealtimeNotificationMessage>.Failure(result.Error);

        var sendMessage = new RealtimeNotificationMessage { };

        return Result<RealtimeNotificationMessage>.Success(sendMessage);
    }
    private async Task<Result<RealtimeNotificationMessage>> HandleComment(NotificationMessageQueue message){
        var result = await _notificationRepository.AddNotification(message);

        if(result.IsError)
            return Result<RealtimeNotificationMessage>.Failure(result.Error);

        var sendMessage = await MessagePacking(message, result.Data);

        if(sendMessage.IsError)
            return Result<RealtimeNotificationMessage>.Failure(sendMessage.Error);


        return Result<RealtimeNotificationMessage>.Success(sendMessage.Data);
    }
    private async Task<Result<RealtimeNotificationMessage>> HandleNewFeedItem(NotificationMessageQueue message){

        var sendMessage = await MessagePacking(message, message.ResourceId!.Value);

        if(sendMessage.IsError)
            return Result<RealtimeNotificationMessage>.Failure(sendMessage.Error);

        return Result<RealtimeNotificationMessage>.Success(sendMessage.Data);
    }
    private async Task<Result<RealtimeNotificationMessage>> HandleFriendRequest(NotificationMessageQueue message){
        var result = await _notificationRepository.AddNotification(message);

        if(result.IsError)
            return Result<RealtimeNotificationMessage>.Failure(result.Error);

        var sendMessage = await MessagePacking(message, result.Data);

        if(sendMessage.IsError)
            return Result<RealtimeNotificationMessage>.Failure(sendMessage.Error);

        return Result<RealtimeNotificationMessage>.Success(sendMessage.Data);
    }

    private async Task<Result<RealtimeNotificationMessage>> HandleFriendRequestAccepted(NotificationMessageQueue message){
        var removeFriendRequest = await _notificationRepository.RemoveFriendRequestNotification(message);

        if(removeFriendRequest.IsError)
            return Result<RealtimeNotificationMessage>.Failure(removeFriendRequest.Error);

        var addFriend = await _notificationRepository.AddNotification(message);

        if(addFriend.IsError)
            return Result<RealtimeNotificationMessage>.Failure(addFriend.Error);

        var sendMessage = await MessagePacking(message, addFriend.Data);

        if(sendMessage.IsError)
            return Result<RealtimeNotificationMessage>.Failure(sendMessage.Error);

        return Result<RealtimeNotificationMessage>.Success(sendMessage.Data);
    }
    private async Task<Result<RealtimeNotificationMessage>> MessagePacking(NotificationMessageQueue message, int id){

        var generatedMessage = NotificationUtils.GenerateNotificationMessage(message.Type, message.ResourceType);
        var username = await _userRepository.GetUsernameById(message.TargetUserId);
        var senderFirstname = await _userRepository.GetFristnameById(message.SenderUserId);
        var senderLastname = await _userRepository.GetLastnameById(message.SenderUserId);
        var senderProfilePhoto = await _userRepository.GetProfilePhotoById(message.SenderUserId);

        if(generatedMessage.IsError) 
            return Result<RealtimeNotificationMessage>.Failure(generatedMessage.Error);

        if(username.IsError) 
            return Result<RealtimeNotificationMessage>.Failure(username.Error);

        if(senderFirstname.IsError) 
            return Result<RealtimeNotificationMessage>.Failure(senderFirstname.Error);

        if(senderLastname.IsError) 
            return Result<RealtimeNotificationMessage>.Failure(senderLastname.Error);

        if(senderProfilePhoto.IsError) 
            return Result<RealtimeNotificationMessage>.Failure(senderProfilePhoto.Error);


        var sendMessage = new RealtimeNotificationMessage
        {
            Id = id,
            Type = message.Type,
            Message = generatedMessage.Data!,
            SenderFirstname = senderFirstname.Data,
            SenderLastname = senderLastname.Data,
            SenderProfilePhoto = senderProfilePhoto.Data,
            CreatedAt = message.Timestamp
        };

        return Result<RealtimeNotificationMessage>.Success(sendMessage);

    }
}