using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CrtBot.WebhookReceiver.Services;

public class MarketMonitor : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MarketMonitor> _logger;

    public MarketMonitor(IServiceProvider services, ILogger<MarketMonitor> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var tracker = scope.ServiceProvider.GetRequiredService<IPositionTracker>();
            var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();
            var riskManager = scope.ServiceProvider.GetRequiredService<IRiskManager>();

            var open = tracker.GetOpenPositions().ToList();
            foreach (var pos in open)
            {
                var price = await broker.GetCurrentPriceAsync(pos.Ticker);
                await CheckPosition(pos, price, tracker, broker, riskManager);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckPosition(Position pos, decimal price, IPositionTracker tracker, IBrokerService broker, IRiskManager riskManager)
    {
        // SL
        bool slHit = pos.Direction == PositionDirection.Long ? price <= pos.StopLoss : price >= pos.StopLoss;
        if (slHit)
        {
            await broker.ClosePositionAsync(pos.Ticker, pos.RemainingQuantity);

            var pnl = pos.Direction == PositionDirection.Long
                ? (price - pos.EntryPrice) * pos.RemainingQuantity
                : (pos.EntryPrice - price) * pos.RemainingQuantity;
            riskManager.RecordResult(pnl);

            tracker.ClosePosition(pos.Id);
            return;
        }

        // TP
        foreach (var tp in pos.TakeProfitLevels.Where(t => !t.IsHit))
        {
            bool tpHit = pos.Direction == PositionDirection.Long ? price >= tp.Price : price <= tp.Price;
            if (tpHit)
            {
                decimal closeQty = pos.InitialQuantity * (tp.QuantityPercentage / 100m);
                if (closeQty > pos.RemainingQuantity) closeQty = pos.RemainingQuantity;

                await broker.ClosePositionAsync(pos.Ticker, closeQty);

                var pnl = pos.Direction == PositionDirection.Long
                    ? (price - pos.EntryPrice) * closeQty
                    : (pos.EntryPrice - price) * closeQty;
                riskManager.RecordResult(pnl);

                tp.IsHit = true;
                pos.RemainingQuantity -= closeQty;

                if (pos.RemainingQuantity <= 0.0001m)
                {
                    tracker.ClosePosition(pos.Id);
                }
                else
                {
                    // CRT BE logic: move SL to breakeven after TP1 is hit
                    if (pos.TakeProfitLevels[0] == tp)
                    {
                        pos.StopLoss = pos.EntryPrice;
                    }
                    tracker.UpdatePosition(pos);
                }
            }
        }
    }
}