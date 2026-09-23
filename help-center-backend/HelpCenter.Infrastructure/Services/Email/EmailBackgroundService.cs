using HelpCenter.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Infrastructure.Services.Email;

public sealed class EmailBackgroundService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly SemaphoreSlim _throttle = new(5); // max 5 concurrent SMTP connections

    public EmailBackgroundService(EmailQueue queue, IServiceScopeFactory scopeFactory, ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            _ = SendAsync(job, stoppingToken);
        }
    }

    private async Task SendAsync(EmailJob job, CancellationToken stoppingToken)
    {
        await _throttle.WaitAsync(stoppingToken);
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            switch (job.Type)
            {
                case EmailJobType.NewRequest:
                    await emailService.SendNewRequestNotificationAsync(
                        job.ToEmail, job.RecipientName, job.SenderName, job.RequestTitle, stoppingToken);
                    break;

                case EmailJobType.ExpertAssigned:
                    await emailService.SendExpertAssignedNotificationAsync(
                        job.ToEmail, job.RecipientName, job.SenderName, job.RequestTitle, stoppingToken);
                    break;
            }

            _logger.LogInformation("Mail gönderildi [{Type}] -> {Email} | Talep: {Title}", job.Type, job.ToEmail, job.RequestTitle);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mail gönderilemedi. Alıcı: {Email}, Talep: {Title}", job.ToEmail, job.RequestTitle);
        }
        finally
        {
            _throttle.Release();
        }
    }
}
