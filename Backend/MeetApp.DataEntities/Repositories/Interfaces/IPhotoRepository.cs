using MeetApp.DataEntities.Common;

namespace MeetApp.DataEntities.Repositiories.Interfaces;

public interface IPhotoRepository
{
    Task<Result<bool>> IsResized(string fileId, string type);
    Task<int> GetOwnerPhoto(int photoId);
    Task<int> GetOwnerPost(int postId);
}