using API.Services.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using MeetApp.DataEntities.Common;

namespace API.Services;

public class MediaStorageService(IAmazonS3 s3Client) : IMediaStorageService{

    private readonly string _bucketName = "catalin-first-bucket";

    public async Task<Result<string>> UploadFileAsync(IFormFile file, string path)
    {

        string fileKey = Guid.NewGuid().ToString();
        string key = $"{path}/{fileKey}";

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = memoryStream
        };

        var response = await s3Client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            return Result<string>.Success(fileKey);
        else
            return Result<string>.Failure($"Error: {response.HttpStatusCode}");

    }

    public async Task<(Stream stream, string ContentType)> GetFileAsync(string id, string key){

        try{

            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await s3Client.GetObjectAsync(request);

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var contentType = response.Headers["Content-Type"];
            return (memoryStream, contentType);
        }
        catch(AmazonS3Exception){
            throw new FileNotFoundException("File not found!");
        }

    }
    


}