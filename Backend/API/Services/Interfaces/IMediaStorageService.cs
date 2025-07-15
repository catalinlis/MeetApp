namespace API.Services.Interfaces;

public interface IMediaStorageService{
    Task<(bool success, string FileKey)> UploadFileAsync(IFormFile file, string path);
    Task<(Stream stream, string ContentType)> GetFileAsync(string id, string path);
}