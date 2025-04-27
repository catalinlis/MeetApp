using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace API.Controllers;

public class MessageController(IChatService chatService,
                               UserManager<AppUser> userManager) : BaseController{

    [HttpPost("{senderUser}/{receiverUser}")]
    public async Task<IActionResult> AddMessage(string senderUser, string receiverUser, [FromQuery] string message){

        var sender = await userManager.FindByNameAsync(senderUser);
        var receiver = await userManager.FindByNameAsync(receiverUser);

        if(sender != null && receiver != null){

            var chatMessage = new ChatMessageDynamo{
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Message = message
            };

            var inserted = await chatService.SaveMessage(chatMessage);

            if(inserted){
                await chatService.AddChat(sender, receiver);
                return Ok();
            }
            else
                return StatusCode(500, "The message was not inserted");
        }

        return BadRequest("No such users");
    }

    [HttpGet("{senderUser}/{receiverUser}")]
    public async Task<IActionResult> GetMessages(string senderUser, string receiverUser, [FromQuery] int offset, [FromQuery] int fetch, [FromQuery] string sortKey = ""){
        
        var sender = await userManager.FindByNameAsync(senderUser);
        var receiver = await userManager.FindByNameAsync(receiverUser);

        if(sender != null && receiver != null){

            var chatMessagesDynamo = await chatService.GetMessages(sender.Id, receiver.Id, fetch, sortKey);
            var chatMessages = new List<ChatMessage>();


            foreach(var messageDynamo in chatMessagesDynamo){
                var senderName = messageDynamo.SenderId == sender.Id ? sender.UserName : receiver.UserName;
                var receiverName = messageDynamo.ReceiverId == receiver.Id ? receiver.UserName : sender.UserName;

                var message = new ChatMessage{
                    Sender = senderName!,
                    Receiver = receiverName!,
                    Message = messageDynamo.Message,
                    SentDate = messageDynamo.SentDate
                };

                chatMessages.Add(message);
            }

            var lastSortKey = "";
            var end = true;

            if(chatMessagesDynamo.Count > 0 ){
                lastSortKey = chatMessagesDynamo[chatMessagesDynamo.Count-1].SortKey;
                end = false;
            }
            
            return Ok(new { Messages=chatMessages, LastSortKey=lastSortKey, End=end });

        }
        return BadRequest("No such users");
    }

    [HttpGet("chats/{username}")]
    public async Task<ActionResult<List<UserMember>>> GetChats(string username){
        
        var user = await userManager.FindByNameAsync(username);

        if(user != null){

            var usersChats = await chatService.GetChats(user);

            return Ok(usersChats);
        }

        return BadRequest("There is no such user");
    }

    [HttpPut("set/read/{receiver}/{sender}")]
    public async Task<IActionResult> SetReadMessages(string receiver, string sender){
        var receiverUser = await userManager.FindByNameAsync(receiver);
        var senderUser = await userManager.FindByNameAsync(sender);

        if(receiverUser != null && senderUser != null){
            await chatService.SetReadMessages(receiverUser.Id, senderUser.Id);

            return Ok();
        }

        return BadRequest("No such users");
    }

    [HttpGet("unread/{receiver}/{sender}")]
    public async Task<IActionResult> GetNumberOfUnreadMessages(string receiver, string sender){
        var receiverUser = await userManager.FindByNameAsync(receiver);
        var senderUser = await userManager.FindByNameAsync(sender);

        if(receiverUser != null && senderUser != null){
            var count = await chatService.GetUnreadMessages(receiverUser.Id, senderUser.Id);

            return Ok(new {Count=count});
        }

        return BadRequest("No such users");

    }
}