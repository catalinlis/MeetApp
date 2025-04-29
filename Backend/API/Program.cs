using System.Text;
using DotNetEnv;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using API.Repositories.Interfaces;
using API.Repositories;
using API.Services.Interfaces;
using API.UserServices;
using API.Services;
using Amazon.DynamoDBv2;
using API.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<EnvironmentVariables>();
var envs = builder.Services.BuildServiceProvider().GetRequiredService<EnvironmentVariables>();

builder.Services.AddDbContext<DataContext>(options => {
    options.UseNpgsql(envs["DEFAULT_DATABASE_CONNECTION"]);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(envs["REDIS_CONNECTION"])
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<CloudFrontService>();
builder.Services.AddHttpClient();

var credential = new BasicAWSCredentials(envs["AWS_ACCESS_KEY_ID"], envs["AWS_SECRET_KEY_ACCESS_ID"]);

var awsOptions = new AWSOptions{
    Credentials = credential,
    Region = RegionEndpoint.EUNorth1
};

builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(credential, RegionEndpoint.EUNorth1));

builder.Services.AddIdentityCore<AppUser>()
                .AddEntityFrameworkStores<DataContext>();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    var secret = envs["JWT_SECRET"];
    var issuers = envs["VALID_ISSUERS"];
    var audiences = envs["VALID_AUDIENCES"];

    if(string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuers) || string.IsNullOrEmpty(audiences))
        throw new ApplicationException("Jwt configuration is not set");
    
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = issuers,
        ValidAudience = audiences,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };

    options.Events = new JwtBearerEvents{
        OnMessageReceived = context => {
            var accessToken = context.Request.Query["access_token"];

            if(!string.IsNullOrEmpty(accessToken) &&
                    (context.HttpContext.Request.Path.StartsWithSegments("/userStatusHub") ||
                    context.HttpContext.Request.Path.StartsWithSegments("/chatHub")))
                context.Token = accessToken;

            return Task.CompletedTask;
        }
    };
});



var app = builder.Build();

# TODO: Add env variable for CORS policy
app.UseCors(x => x.WithOrigins(envs["CORS_POLICY"]).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

//app.MapHub<UserStatusHub>("/userStatusHub");
app.MapHub<UserStatusRedisHub>("/userStatusHub");
app.MapHub<ChatHub>("/chatHub");


app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await SeedData.SeedInterest(context);
}catch(Exception ex){
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migrations");
}

app.Run();
