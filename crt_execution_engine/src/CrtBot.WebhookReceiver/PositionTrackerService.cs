using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CrtBot.WebhookReceiver.Services;

public class PositionTrackerService : IPositionTracker
{
    private readonly ConcurrentDictionary<Guid, Position> _positions = new();
    private readonly IBrokerService _broker;
    private readonly ILogger<PositionTrackerService> _logger;

    public PositionTrackerService(IBrokerService broker, ILogger<PositionTrackerService> logger)
    {
        _broker = broker;
        _logger = logger;
    }

    public void AddPosition(Position position)
    {
        _positions.TryAdd(position.Id, position);
    }

    public IEnumerable<Position> GetOpenPositions()
    {
        return _positions.Values.Where(p => !p.IsClosed);
    }

    public void UpdatePosition(Position position)
    {
        _positions[position.Id] = position;
    }

    public void ClosePosition(Guid positionId)
    {
        if (_positions.TryGetValue(positionId, out var pos))
        {
            pos.IsClosed = true;
        }
    }

    public async Task CloseAllPositionsAsync(string ticker)
    {
        var open = GetOpenPositions().Where(p => p.Ticker == ticker).ToList();
        foreach (var pos in open)
        {
            await _broker.ClosePositionAsync(pos.Ticker, pos.RemainingQuantity);
            ClosePosition(pos.Id);
        }
    }
}