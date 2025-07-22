using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;

namespace API.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetPhotoComments(int photoId);
    Task<Comment> AddPhotoComment(int photoId, AppUser user, string text);
    Task<IEnumerable<Comment>> GetPostComments(int postId);
    Task<Comment> AddPostComment(int postId, AppUser user, string text);
    Task<bool> ExistPhotoId(int photoId);
    Task<bool> ExistPostId(int postId);
}