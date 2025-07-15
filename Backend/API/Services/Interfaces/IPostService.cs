using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;

namespace API.Services.Interfaces;

public interface IPostService{
    Task<ServiceResult<bool>> LikePhoto(AppUser user, Photo photo);
    Task<ServiceResult<bool>> LikePost(AppUser user, Post post);
    Task<ServiceResult<FeedItem>> AddPhoto(AppUser user, IFormFile file, string text);
    Task<ServiceResult<FeedItem>> AddPost(AppUser user, IFormFile? file, string text, List<string> interestKeys);
    Task<ServiceResult<List<FeedItem>>> GetFeed(AppUser user);
}