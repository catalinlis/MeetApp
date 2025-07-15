using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Interfaces;

public class CommentService(DataContext context) : ICommentService{
    public async Task<bool> ExistPhotoId(int photoId){
        var photo = await context.Photos.FirstOrDefaultAsync(x => x.Id == photoId);

        if(photo == null)
            return false;
        else
            return true;
    }
    public async Task<bool> ExistPostId(int postId){
        var post = await context.Posts.FirstOrDefaultAsync(x => x.Id == postId);

        if(post == null)
            return false;
        else
            return true;
    }
    public async Task<IEnumerable<Comment>> GetPhotoComments(int photoId){

        var comments = await context.Photos.Include(x => x.Comments)
                                           .ThenInclude(x => x.CreatedByUser) 
                                           .FirstOrDefaultAsync(x => x.Id == photoId);

        var commentsDTO = new List<Comment>();

        foreach(var comment in comments!.Comments){
            var commentDTO = new Comment
            {
                Username = comment.CreatedByUser.UserName,
                Content = comment.Text,
                AddedAt = comment.CreatedAt
            };

            commentsDTO.Add(commentDTO);
        }

        return commentsDTO.OrderByDescending(x => x.AddedAt);
    }
    public async Task<Comment> AddPhotoComment(int photoId, AppUser user, string text){

        var photo = await context.Photos.Include(x => x.Comments).FirstOrDefaultAsync(x => x.Id == photoId);

        var comment = new PhotoComment
        {
            Text = text,
            CreatedByUser = user,
            Photo = photo!
        };

        photo!.CommentsCount++;
        context.PhotosComments.Add(comment);
        await context.SaveChangesAsync();

        var commentDTO = new Comment
        {
            Username = user.UserName,
            Content = comment.Text,
            AddedAt = comment.CreatedAt
        };

        return commentDTO;

    }
    public async Task<IEnumerable<Comment>> GetPostComments(int postId){
        var comments = await context.Posts.Include(x => x.Comments)
                                          .ThenInclude(x => x.CreatedByUser)
                                          .FirstOrDefaultAsync(x => x.Id == postId);

        var commentsDTO = new List<Comment>();

        foreach(var comment in comments!.Comments){
            var commentDTO = new Comment
            {
                Username = comment.CreatedByUser.UserName,
                Content = comment.Text,
                AddedAt = comment.CreatedAt
            };

            commentsDTO.Add(commentDTO);
        }

        return commentsDTO.OrderByDescending(x => x.AddedAt);
    }
    public async Task<Comment> AddPostComment(int postId, AppUser user, string text){
        var post = await context.Posts.Include(x => x.Comments).FirstOrDefaultAsync(x => x.Id == postId);

        var comment = new PostComment
        {
            Text = text,
            CreatedByUser = user,
            Post = post!,
        };

        post!.CommentsCount++;
        context.PostsComments.Add(comment);
        await context.SaveChangesAsync();

        var commentDTO = new Comment
        {
            Username = user.UserName,
            Content = comment.Text,
            AddedAt = comment.CreatedAt
        };

        return commentDTO;
    }
}