using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrtBot.Scheduler;

public class FridayCloseScheduler : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<FridayCloseScheduler> _logger;

    public FridayCloseScheduler(IServiceProvider services, ILogger<FridayCloseScheduler> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Friday Close Scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow.AddHours(-4); // UTC-4
            if (now.DayOfWeek == DayOfWeek.Friday && now.Hour == 15 && now.Minute < 1)
            {
                _logger.LogInformation("Friday 15:00 UTC-4 reached. Closing all positions.");
                using var scope = _services.CreateScope();
                var tracker = scope.ServiceProvider.GetRequiredService<IPositionTracker>();
                var openPositions = tracker.GetOpenPositions().ToList();
                foreach (var pos in openPositions)
                {
                    await tracker.CloseAllPositionsAsync(pos.Ticker);
                }
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}