using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, 
                               EnvironmentVariables variables) : BaseController{

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUser registerUser){
        if(ModelState.IsValid){
            var existUser = await userManager.FindByNameAsync(registerUser.Username);

            if(existUser != null){
                ModelState.AddModelError("Username", "This username is already taken");
                return BadRequest(ModelState);
            }

            Console.WriteLine(DateTimeOffset.UtcNow);

            var user = new AppUser{
                Firstname = registerUser.Firstname,
                Lastname = registerUser.Lastname,
                UserName = registerUser.Username,
                Email = registerUser.Email,
                DateOfBirth = registerUser.DateOfBirthday,
                Country = registerUser.Country,
                City = registerUser.City,
                Gender = registerUser.Gender,
                RegisterStep = 1
            };

            Console.WriteLine(user.DateOfBirth);

            var result = await userManager.CreateAsync(user, registerUser.Password);

            if(result.Succeeded){
                var token = GenerateToken(user.UserName);
                return Ok(new {user.Firstname, user.Lastname, user.UserName, user.RegisterStep, token});
            }

            foreach(var Error in result.Errors)
                ModelState.AddModelError(string.Empty, Error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUser loginUser){
        if(ModelState.IsValid){
            var user = await userManager.FindByNameAsync(loginUser.Username);

            if(user != null){
                if(await userManager.CheckPasswordAsync(user, loginUser.Password)){
                    var token = GenerateToken(loginUser.Username);
                    return Ok(new {user.Firstname, user.Lastname, user.UserName, user.RegisterStep, user.ProfilePhoto, token});
                }
            }

            ModelState.AddModelError("Login", "Username or password is invalid");
        }

        return BadRequest(ModelState);
    }

    private string? GenerateToken(string username){

        var secret = variables["JWT_SECRET"];
        var issuers = variables["VALID_ISSUERS"];
        var audiences = variables["VALID_AUDIENCES"];

        if(string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuers) || string.IsNullOrEmpty(audiences))
            throw new ApplicationException("Jwt configuration is not set");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var securityTokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(new [] {
                new Claim(ClaimTypes.Name, username),
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = issuers,
            Audience = audiences,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = jwtTokenHandler.CreateToken(securityTokenDescriptor);
        var token = jwtTokenHandler.WriteToken(securityToken);

        return token;
     }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> Delete(int id){

        if(ModelState.IsValid){
            var user = await userManager.FindByIdAsync(id.ToString());

            if(user == null){
                ModelState.AddModelError("Id", "There is no such id");
                return BadRequest(ModelState);
            }

            await userManager.DeleteAsync(user);
            return Ok(new {user.Id});
        }

        return BadRequest(ModelState);

    } 

}