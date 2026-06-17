using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.Logging;

namespace CrtBot.Broker;

public class MockBrokerService : IBrokerService
{
    private readonly ILogger<MockBrokerService> _logger;
    private readonly Dictionary<string, decimal> _prices = new()
    {
        { "EURUSD", 1.0850m },
        { "BTCUSD", 65000m },
        { "XAUUSD", 2350m }
    };

    public MockBrokerService(ILogger<MockBrokerService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ExecuteOrderAsync(string ticker, PositionDirection direction, decimal quantity, decimal price)
    {
        _logger.LogInformation("MOCK BROKER: Executing {Direction} order for {Ticker}: {Quantity} @ {Price}", direction, ticker, quantity, price);
        return Task.FromResult(true);
    }

    public Task<decimal> GetCurrentPriceAsync(string ticker)
    {
        if (_prices.TryGetValue(ticker, out var price))
        {
            return Task.FromResult(price);
        }
        return Task.FromResult(1.0m); // Default fallback
    }
}
