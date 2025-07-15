using MeetApp.DataEntities.Data;
using MeetApp.DataEntities.Entities;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Notification.Helpers;
using Notification.Services;
using DotNetEnv;
using NotificationQueue.Services;
using NotificationQueue.Services.Interfaces;
using MeetApp.DataEntities.Configurations;
using Notification.Services.Interfaces;
using MeetApp.DataEntities.Repositiories.Interfaces;
using MeetApp.DataEntities.Repositiories;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddSingleton<EnvironmentVariables>();
var envs = builder.Services.BuildServiceProvider().GetRequiredService<EnvironmentVariables>();

builder.Services.AddDbContext<DataContext>(options => {
    options.UseNpgsql(envs[ConfigurationProperties.DBConnection]);
});

builder.Services.AddIdentityCore<AppUser>()
                .AddEntityFrameworkStores<DataContext>();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(envs[ConfigurationProperties.RedisConnection])
);

builder.Services.Configure<RabbitMqConfiguration>(options => 
{
    options.HostName = envs[RabbitMqConfigurationProperties.Hostname];
    options.Username = envs[RabbitMqConfigurationProperties.Username];
    options.Password = envs[RabbitMqConfigurationProperties.Password];
});


builder.Services.AddSignalR().AddStackExchangeRedis(envs[ConfigurationProperties.RedisConnection], options => {
    options.Configuration.ChannelPrefix = "MeetAppNotification";
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IConsumerQueueService, ConsumerQueueService>();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddHostedService<NotificationService>();

builder.Services.AddCors();

builder.Services.AddJwtAuthentication(envs[ConfigurationProperties.JwtSecret],
                                      envs[ConfigurationProperties.ValidIssuer],
                                      envs[ConfigurationProperties.ValidAudiences]);

var app = builder.Build();

app.UseCors(x => x.WithOrigins(envs[ConfigurationProperties.CorsPolicy]).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationsHub");

using(var scope = app.Services.CreateScope()){
    var services = scope.ServiceProvider;

    try{
        var consumerService = services.GetRequiredService<IConsumerQueueService>();
        await consumerService.InitAsync();
    } catch (Exception ex){
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during initialization of RabbitMq connection");
    }
}

app.Run();
