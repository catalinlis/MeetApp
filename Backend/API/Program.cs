using System.Text;
using DotNetEnv;
using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using MeetApp.DataEntities.Repositiories.Interfaces;
using MeetApp.DataEntities.Repositiories;
using API.Services.Interfaces;
using API.UserServices;
using API.Services;
using Amazon.DynamoDBv2;
using API.Hubs;
using API.Helpers;
using StackExchange.Redis;
using NotificationQueue.Services;
using NotificationQueue.Services.Interfaces;
using MeetApp.DataEntities.Configurations;
using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<EnvironmentVariables>();
var envs = builder.Services.BuildServiceProvider().GetRequiredService<EnvironmentVariables>();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseNpgsql(envs.Get(ConfigurationProperties.DBConnection), sql => sql.MigrationsAssembly("API")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(envs.Get(ConfigurationProperties.RedisConnection))
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMediaStorageService, MediaStorageService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddSingleton<CloudFrontService>();
builder.Services.AddHttpClient();

var credential = new BasicAWSCredentials(envs.Get(ConfigurationProperties.AwsAccessKeyId), envs.Get(ConfigurationProperties.AwsSecretKeyAccessId));

var awsOptions = new AWSOptions{
    Credentials = credential,
    Region = RegionEndpoint.EUNorth1
};

builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(credential, RegionEndpoint.EUNorth1));
builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(credential, RegionEndpoint.EUNorth1));
builder.Services.AddScoped<ISQSService>(sp =>
{
    var sqsClient = sp.GetRequiredService<IAmazonSQS>();
    return new SQSService(sqsClient, envs.Get(ConfigurationProperties.SqsUrl));
});


builder.Services.AddIdentityCore<AppUser>()
                .AddEntityFrameworkStores<DataContext>();

builder.Services.Configure<RabbitMqConfiguration>(options =>
{
    options.HostName = envs[RabbitMqConfigurationProperties.Hostname];
    options.Username = envs[RabbitMqConfigurationProperties.Username];
    options.Password = envs[RabbitMqConfigurationProperties.Password];
});

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IQueueService, QueueService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var secret = envs.Get(ConfigurationProperties.JwtSecret);
    var issuers = envs.Get(ConfigurationProperties.ValidIssuer);
    var audiences = envs.Get(ConfigurationProperties.ValidAudiences);

    if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuers) || string.IsNullOrEmpty(audiences))
        throw new ApplicationException("Jwt configuration is not set");

    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = issuers,
        ValidAudience = audiences,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) &&
                    (context.HttpContext.Request.Path.StartsWithSegments("/userStatusHub") ||
                    context.HttpContext.Request.Path.StartsWithSegments("/chatHub")))
                context.Token = accessToken;

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// TODO: Add env variable for CORS policy
app.UseCors(x => x.WithOrigins(envs.Get(ConfigurationProperties.CorsPolicy)).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

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

try{
    var consumerService = services.GetRequiredService<IQueueService>();
    await consumerService.InitAsync();
} catch (Exception ex){{
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during initialization of RabbitMq connection");
    }}

app.Run();
