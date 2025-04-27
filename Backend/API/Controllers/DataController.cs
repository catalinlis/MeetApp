using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using API.Data;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;


public class DataController(DataContext context, 
                            UserManager<AppUser> userManager,
                            IAmazonS3 s3Client,
                            CloudFrontService cloudFront) : BaseController{

    [HttpGet("check")]
    public IActionResult Check(){
        var username = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        return Ok(new {username});
    }

    [HttpPost("upload-image/{username}")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, string username){
        if(file == null || file.Length <= 0)
            return BadRequest("Invalid file");

        string bucketName = "catalin-first-bucket";
        string fileKey = Guid.NewGuid().ToString();

        using (var memoryStream = new MemoryStream()){
            await file.CopyToAsync(memoryStream);

            var putRequest = new PutObjectRequest{
                BucketName = bucketName,
                Key = "uploads/" + fileKey,
                InputStream = memoryStream
            };

            var response = await s3Client.PutObjectAsync(putRequest);

            if(response.HttpStatusCode == System.Net.HttpStatusCode.OK){
                if(username != null){
                    var user = await userManager.FindByNameAsync(username);
                    user!.RegisterStep = 2;
                    user!.ProfilePhoto = fileKey;
                    var result = await userManager.UpdateAsync(user);
                    if(result.Succeeded)
                        return Ok(new { RegisterStep = 2, profilePhoto = user.ProfilePhoto });
                }

                return BadRequest("Could not save the token image in database");
            }
            else
                return StatusCode(500, "Error uploading file to S3");
        }
    }

    [HttpGet("get-image/{id}")]
    public async Task<IActionResult> GetPhoto(string id){

        var filename = id;
        var bucketName = "catalin-first-bucket";
        try{
            var request = new GetObjectRequest{
                BucketName = bucketName,
                Key = "uploads/" + filename
            };

            using var response = await s3Client.GetObjectAsync(request);
            var responseStream = response.ResponseStream;
            var memoryStream = new MemoryStream();

            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return File(memoryStream, response.Headers["Content-Type"], filename);
        } catch(AmazonS3Exception ex){
            return NotFound(new { Message = ex.Message });
        } catch(Exception ex){
            return StatusCode(500, new { Message = ex.Message });
        }
    }


    [HttpGet("sign-url/{id}")]
    public async Task<IActionResult> GetPhotoUrl(string id){

        string signedUrl = cloudFront.SignUrl(id);

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