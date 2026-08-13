using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Infrastructure.Services;

public class FeeOverdueNotificationService : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FeeOverdueNotificationService> _logger;

    public FeeOverdueNotificationService(
        IServiceScopeFactory scopeFactory,
        ILogger<FeeOverdueNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueFeesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fee overdue notification check failed.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckOverdueFeesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateTime.Now.Date;
        var structures = (await unitOfWork.FeeStructures.FindAsync(s =>
            s.IsRequired && !s.IsDeleted &&
            (s.FineEndDate.HasValue
                ? s.FineEndDate.Value.Date < today
                : s.Installments.Any(i => i.DueDate.Date.AddDays(s.FineAfterDays) < today &&
                    i.PaidAmount < i.Amount)))).ToList();

        foreach (var structure in structures)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            var marker = $"fee-overdue:{structure.Id}:{structure.ClassRoomId}";
            if (await unitOfWork.Notifications.AnyAsync(n => n.Data == marker))
                continue;

            var hasPending = await unitOfWork.FeeInstallments.AnyAsync(i =>
                i.FeeStructureId == structure.Id && i.PaidAmount < i.Amount);
            if (!hasPending)
                continue;

            var title = "Fee payment overdue";
            var message = $"Your fee '{structure.Name}' is overdue and a late fine of {structure.FineAmount:C} applies. Please pay as soon as possible.";
            await notificationService.NotifyStudentsOfClassAsync(
                structure.ClassRoomId, title, message, NotificationType.Warning, link: "/fees", data: marker);
        }
    }
}
