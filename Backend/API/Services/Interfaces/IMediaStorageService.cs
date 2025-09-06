using MeetApp.DataEntities.Common;

namespace API.Services.Interfaces;

public interface IMediaStorageService{
    Task<Result<string>> UploadFileAsync(IFormFile file, string path);
    Task<(Stream stream, string ContentType)> GetFileAsync(string id, string key);
}