using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Infrastructure.Services;

public class AutoClockOutService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AutoClockOutTime = new TimeSpan(18, 30, 0);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoClockOutService> _logger;

    public AutoClockOutService(IServiceScopeFactory scopeFactory, ILogger<AutoClockOutService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAutoClockOutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto clock-out check failed.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessAutoClockOutsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var records = await unitOfWork.Attendances.FindAsync(a =>
            a.CheckInTime != null &&
            a.CheckOutTime == null &&
            a.Date.Date < today &&
            !a.IsDeleted);

        foreach (var record in records)
        {
            record.CheckOutTime = AutoClockOutTime;
            record.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.Attendances.UpdateAsync(record);
        }

        if (records.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Auto clock-out completed: {Count} records updated.", records.Count);
        }
    }
}
