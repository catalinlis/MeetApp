using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.Entities;
using API.Services;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.Configurations;
using MeetApp.DataEntities.Repositiories.Interfaces;

namespace API.Controllers;


public class DataController(DataContext context, 
                            UserManager<AppUser> userManager,
                            CloudFrontService cloudFront,
                            IMediaStorageService storageService,
                            IPhotoRepository photoRepository,
                            ISQSService sqsService,
                            ILogger<DataController> logger) : BaseController{

    [HttpGet("check")]
    public IActionResult Check(){
        var username = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        return Ok(new {username});
    }

    [HttpPost("upload-image/{username}")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, string username){
        if(file == null || file.Length == 0)
            return BadRequest("Invalid file");

        if(username == null)
            return BadRequest("Invalid username provided");

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("There is no such user");

        var result = await storageService.UploadFileAsync(file, BucketKeys.PhotoPath);

        if(result.IsSuccess){
            var photo = new Photo
            {
                PhotoId = result.Data!,
                AddedBy = user
            };
            
            user.Photos.Add(photo);
            user.RegisterStep = 2;
            user.ProfilePhoto = result.Data!;

            var updateResult = await userManager.UpdateAsync(user);

            var bucketKey = $"{BucketKeys.PhotoPath}/{result.Data!}";
            
            var queueMessageResult = await sqsService.SendQueueMessage(photo.Id, bucketKey, NotificationResourceType.Photo);

            if (queueMessageResult.IsError)
                logger.LogWarning(queueMessageResult.Error);

            if (updateResult.Succeeded)
                return Ok(new { RegisterStep = 2, profilePhoto = user.ProfilePhoto });
            else
                return BadRequest("Could not save the token image in database");
        }
        else
            return StatusCode(500, result.Error);
    }
    

    [HttpGet("get-image/{id}")]
    public async Task<IActionResult> GetPhoto(string id){

        var filename = id;
        var path = "photos";

        try{
            var (stream, contentType) = await storageService.GetFileAsync(filename, path);

            return File(stream, contentType, filename);

        } catch(FileNotFoundException ex){
            return NotFound(new { Message = ex.Message });
        } catch(Exception ex){
            return StatusCode(500, new { Message = ex.Message });
        }
    }


    [HttpGet("sign-url/{fileKey}")]
    public async Task<IActionResult> GetPhotoUrl(string fileKey){
        Console.WriteLine(fileKey);
        
        var resized = await photoRepository.IsResized(fileKey, NotificationResourceType.Photo);

        if (resized.IsError)
            return NotFound(resized.Error);

        var path = resized.Data ? BucketKeys.ResizedPhotoPath : BucketKeys.PhotoPath;

        string signedUrl = cloudFront.SignUrl(fileKey, path);

        return Ok(new { signedUrl = signedUrl});
    }


    [HttpGet("interests")]
    public async Task<IActionResult> GetInterests(){
        var interests = await context.Interests.ToListAsync();

        if(interests == null) return BadRequest("There are no interests in database");

        return Ok(interests);
    }

    [HttpDelete("interests")]
    public async Task<IActionResult> DeleteInterests(){
        var interests = await context.Interests.ToListAsync();

        foreach(var interest in interests){
            context.Interests.Remove(interest);
        }

        await context.SaveChangesAsync();

        return Ok(new { RegisterStep = 2 });
    }


    [HttpPost("interests/add/{username}")]
    public async Task<IActionResult> AddInterests([FromBody] List<InterestModel> IncomingInterests, string username){
       
        if(IncomingInterests == null || !IncomingInterests.Any())
            return BadRequest("The interests list is empty or invalid");

        var user = await userManager.FindByNameAsync(username);

        if(user == null)
            return BadRequest("Specified user is not found");

        var UserInterests = await context.Users.Include(x => x.Interests).FirstAsync(x => x.Id == user.Id);

        foreach(var IncomingInterest in IncomingInterests){
            var interest = await context.Interests.FirstOrDefaultAsync(x => x.InterestKey == IncomingInterest.InterestKey);

            if(interest == null)
                return BadRequest("No such interest in the table");
            
            if(!UserInterests.Interests.Any(x => x.InterestKey == interest.InterestKey)){
                user.Interests.Add(interest);
            }
        }

        user.RegisterStep = 3;

        await userManager.UpdateAsync(user);

        return Ok(new { RegisterStep = 3 });
    }

}