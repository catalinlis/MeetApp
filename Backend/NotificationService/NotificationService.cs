using Notification.Services.Interfaces;

namespace Notification.Services;
public class NotificationService : BackgroundService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IConsumerQueueService _queueService;

    public NotificationService(ILogger<NotificationService> logger, IConsumerQueueService queueService)
    {
        _queueService = queueService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Service started");

        await _queueService.InitAsync();

        await _queueService.ReadMessage();
    }

    public override async Task StopAsync(CancellationToken cancellationToken){
        _logger.LogInformation("Notification Service stoppoing");
        await _queueService.CloseAsync();
        await base.StopAsync(cancellationToken);
    }

}