using API.DTOs;
using API.Entities;

namespace API.Services;

public interface IChatService{

    Task<bool> SaveMessage(ChatMessageDynamo message);
    Task<List<ChatMessageDynamo>> GetMessages(int user1, int user2, int pageNumber, string sortKey);
    Task<List<UserMember>> GetChats(AppUser user);
    Task AddChat(AppUser sender, AppUser receiver);
    Task<int> GetUnreadMessages(int receiver, int sender);
    Task SetReadMessages(int receiver, int sender);

}