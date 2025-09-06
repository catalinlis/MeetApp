using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DataEntities.Repositiories;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;
    public PhotoRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<Result<bool>> IsResized(string fileId, string type)
    {
        switch (type)
        {
            case NotificationResourceType.Photo:
                {
                    var photo = await _context.Photos.FirstOrDefaultAsync(x => x.PhotoId == fileId);

                    if (photo == null)
                        return Result<bool>.Failure("No such photo");

                    return Result<bool>.Success(photo.Resized);
                }
            case NotificationResourceType.Post:
                {
                    var postPhoto = await _context.Posts.FirstOrDefaultAsync(x => x.ImageId == fileId);

                    if (postPhoto == null)
                        return Result<bool>.Failure("no such image");

                    return Result<bool>.Success(postPhoto.Resized);
                }
            default:
                return Result<bool>.Failure("No such type");
        }
    }

    public async Task<int> GetOwnerPhoto(int photoId)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == photoId);

        if (photo != null)
            return photo.AddedById;

        return -1;
    }

    public async Task<int> GetOwnerPost(int postId)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);

        if (post != null)
            return post.CreatedById;

        return -1;
    }
}