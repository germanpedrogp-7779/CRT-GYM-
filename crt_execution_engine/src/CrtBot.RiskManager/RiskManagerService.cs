using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrtBot.RiskManager;

public class RiskConfig
{
    public decimal MaxRiskPerTrade { get; set; } = 100m;
    public decimal MaxDailyLoss { get; set; } = 500m;
    public decimal DefaultQuantity { get; set; } = 0.1m;
}

public class RiskManagerService : IRiskManager
{
    private readonly IOptions<RiskConfig> _config;
    private decimal _dailyPnl = 0;

    public RiskManagerService(IOptions<RiskConfig> config)
    {
        _config = config;
    }

    public Task<(bool IsAllowed, decimal Quantity, string Reason)> EvaluateTradeAsync(TradingViewWebhook webhook)
    {
        if (_dailyPnl <= -_config.Value.MaxDailyLoss)
            return Task.FromResult((false, 0m, "Max daily loss reached"));

        return Task.FromResult((true, _config.Value.DefaultQuantity, "Allowed"));
    }

    public void RecordResult(decimal pnl)
    {
        _dailyPnl += pnl;
    }
}