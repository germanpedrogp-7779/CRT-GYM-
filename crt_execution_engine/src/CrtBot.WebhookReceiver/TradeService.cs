using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.Logging;

namespace CrtBot.WebhookReceiver.Services;

public class TradeService
{
    private readonly IBrokerService _broker;
    private readonly IPositionTracker _tracker;
    private readonly IRiskManager _riskManager;
    private readonly ILogger<TradeService> _logger;

    public TradeService(
        IBrokerService broker,
        IPositionTracker tracker,
        IRiskManager riskManager,
        ILogger<TradeService> logger)
    {
        _broker = broker;
        _tracker = tracker;
        _riskManager = riskManager;
        _logger = logger;
    }

    public async Task HandleWebhookAsync(TradingViewWebhook webhook)
    {
        _logger.LogInformation("Received webhook: {Action} {Ticker}", webhook.Action, webhook.Ticker);

        if (webhook.Action == "CLOSE_ALL")
        {
            await _tracker.CloseAllPositionsAsync(webhook.Ticker);
            return;
        }

        var riskEval = await _riskManager.EvaluateTradeAsync(webhook);
        if (!riskEval.IsAllowed)
        {
            _logger.LogWarning("Trade rejected: {Reason}", riskEval.Reason);
            return;
        }

        var direction = webhook.Action == "BUY" ? PositionDirection.Long : PositionDirection.Short;
        var success = await _broker.ExecuteOrderAsync(webhook.Ticker, direction, riskEval.Quantity, webhook.Price);

        if (success)
        {
            var pos = new Position
            {
                Ticker = webhook.Ticker,
                Direction = direction,
                EntryPrice = webhook.Price,
                InitialQuantity = riskEval.Quantity,
                RemainingQuantity = riskEval.Quantity,
                StopLoss = webhook.Sl,
                TakeProfitLevels = new List<TakeProfitLevel>
                {
                    new() { Price = webhook.Tp1, QuantityPercentage = webhook.Tp1_pct },
                    new() { Price = webhook.Tp2, QuantityPercentage = webhook.Tp2_pct },
                    new() { Price = webhook.Tp3, QuantityPercentage = webhook.Tp3_pct }
                }.Where(tp => tp.Price > 0).ToList()
            };
            _tracker.AddPosition(pos);
            _logger.LogInformation("Position tracked for {Ticker}", webhook.Ticker);
        }
    }
}