using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Entities.ManyToMany;
using MeetApp.DataEntities.Data;
using Microsoft.EntityFrameworkCore;
using API.Services.Interfaces;
using AutoMapper;

namespace API.Services;

public class PostService(DataContext context,
                         IMediaStorageService storageService,
                         IMapper mapper) : IPostService{
    public async Task<ServiceResult<bool>> LikePhoto(AppUser user, Photo photo){
        
        var Liked = photo.LikedByUsers.FirstOrDefault(x => x.UserId == user.Id);

        if(Liked == null){
            
            var photoLike = new PhotoLikes
            {
                UserId = user.Id,
                PhotoId = photo.Id,
                LikedAt = DateTimeOffset.UtcNow
            };

            context.PhotoLikes.Add(photoLike);
            photo.LikesCount++;
        }
        else{
            photo.LikedByUsers.Remove(Liked);
            photo.LikesCount--;
        }

        await context.SaveChangesAsync();

        var hasLiked = Liked == null;

        return ServiceResult<bool>.Ok(hasLiked);
    }
    public async Task<ServiceResult<bool>> LikePost(AppUser user, Post post){
        
        var Liked = post.LikedByUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (Liked == null)
        {

            var postLike = new PostLikes
            {
                UserId = user.Id,
                PostId = post.Id,
                LikedAt = DateTimeOffset.UtcNow
            };

            context.PostLikes.Add(postLike);
            post.LikesCount++;
        }
        else{
            post.LikedByUsers.Remove(Liked);
            post.LikesCount--;
        }

        await context.SaveChangesAsync();

        var hasLiked = Liked == null;

        return ServiceResult<bool>.Ok(hasLiked);
    }
    public async Task<ServiceResult<FeedItem>> AddPhoto(AppUser user, IFormFile file, string text){
        
        var path = "photos";
        string imageId = null!;

        var (success, fileKey) = await storageService.UploadFileAsync(file, path);

        if(success)
            imageId = fileKey;
        else
            return ServiceResult<FeedItem>.Fail("The photo couldn't be uploaded");

        var photo = new Photo
        {
            PhotoId = imageId,
            Text = text,
            AddedBy = user
        };

        var CreatedByUser = await context.Users.FindAsync(user.Id);
        CreatedByUser!.Photos.Add(photo);

        await context.SaveChangesAsync();

        var feedItem = new FeedItem
        {
            Type = "Photo",
            Content = photo.Text,
            Author = photo.AddedBy.UserName,
            ImageUrl = photo.PhotoId,
            CreatedAt = photo.AddedAt,
            CommentsCount = photo.CommentsCount,
            LikesCount = photo.LikesCount,
            PostId = photo.Id
        };

        return ServiceResult<FeedItem>.Ok(feedItem);

    }
    public async Task<ServiceResult<FeedItem>> AddPost(AppUser user, IFormFile? file, string text, List<string> interestKeys){

        var interests = await context.Interests.Where(x => interestKeys.Contains(x.InterestKey)).ToListAsync();
        var path = "posts";
        var existFile = file != null && file.Length > 0 ? true : false;
        string imageId = null!;

        if (existFile)
        {
            var (success, fileKey) = await storageService.UploadFileAsync(file, path);

            if(success)
                imageId = fileKey;
            else
                return  ServiceResult<FeedItem>.Fail("The file couldn't be uploaded");
        }

        var post = new Post
        {
            ImageId = imageId,
            Text = text,
            CreatedBy = user,
            Visibility = "public"
        };

        post.PostInterests = interests.Select(interest => new PostInterest
                        {
                            Post = post,
                            Interest = interest
                        }).ToList();

        var CreatedByUser = await context.Users.FindAsync(user.Id);

        CreatedByUser!.Posts.Add(post);

        await context.SaveChangesAsync();

        var feedItem = new FeedItem
        {
            Type = "Post",
            Content = post.Text,
            Author = post.CreatedBy.UserName,
            ImageUrl = post.ImageId,
            CreatedAt = post.CreatedAt,
            CommentsCount = post.CommentsCount,
            LikesCount = post.LikesCount,
            PostId = post.Id
        };

        return ServiceResult<FeedItem>.Ok(feedItem);
    }
    public async Task<ServiceResult<List<FeedItem>>> GetFeed(AppUser user){
        
        var friendsIds = await context.Friendships
                                .Where(x => x.UserId == user.Id)
                                .Select(x => x.FriendId)
                                .ToListAsync();
        friendsIds.Add(user.Id);

        var posts = await context.Posts
                            .Where(x => friendsIds.Contains(x.CreatedById))
                            .Include(x => x.LikedByUsers)
                            .Include(x => x.CreatedBy)
                            .ToListAsync();

        var postsFeed = new List<FeedItem>();
        
        foreach(var post in posts)
        {
            var hasLiked = post.LikedByUsers.Any(x => x.UserId == user.Id);
            var postInterests = await context.PostInterests
                                                .Where(x => x.PostId == post.Id)
                                                .Select(x => x.Interest)
                                                .ToListAsync();

            var interests = mapper.Map<List<InterestDTO>>(postInterests);

            var postFeed = new FeedItem
            {
                Type = "Post",
                Content = post.Text,
                Author = post.CreatedBy.UserName,
                ImageUrl = post.ImageId,
                CreatedAt = post.CreatedAt,
                LikesCount = post.LikesCount,
                LikedByMe = hasLiked,
                CommentsCount = post.CommentsCount,
                PostId = post.Id,
                Interests = interests
            };

            postsFeed.Add(postFeed);

        } 

        var photos = await context.Photos
                            .Where(x => friendsIds.Contains(x.AddedById))
                            .Include(x => x.LikedByUsers)
                            .Include(x => x.AddedBy)
                            .ToListAsync();

        var photosFeed = new List<FeedItem>();

        foreach(var photo in photos){

            var hasLiked = photo.LikedByUsers.Any(x => x.UserId == user.Id);

            var photoFeed = new FeedItem
            {
                Type = "Photo",
                Content = photo.Text,
                Author = photo.AddedBy.UserName,
                ImageUrl = photo.PhotoId,
                CreatedAt = photo.AddedAt,
                LikesCount = photo.LikesCount,
                CommentsCount = photo.CommentsCount,
                LikedByMe = hasLiked,
                PostId = photo.Id
            };

            photosFeed.Add(photoFeed);

        }

        var feedItems = postsFeed.Concat(photosFeed).OrderByDescending(x => x.CreatedAt).ToList();

        return ServiceResult<List<FeedItem>>.Ok(feedItems);

    }
}

