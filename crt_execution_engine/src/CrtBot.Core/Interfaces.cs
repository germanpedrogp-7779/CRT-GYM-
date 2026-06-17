using CrtBot.Core.Models;

namespace CrtBot.Core.Interfaces;

public interface IBrokerService
{
    Task<bool> ExecuteOrderAsync(string ticker, PositionDirection direction, decimal quantity, decimal price);
    Task<decimal> GetCurrentPriceAsync(string ticker);
}

public interface IRiskManager
{
    Task<bool> ValidateSignalAsync(TradeSignal signal);
    Task<decimal> CalculateQuantityAsync(string ticker, decimal riskAmount, decimal entryPrice, decimal stopLoss);
    Task RecordTradeResultAsync(decimal profitLoss);
}

public interface IPositionTracker
{
    Task<IEnumerable<Position>> GetActivePositionsAsync();
    Task AddPositionAsync(Position position);
    Task UpdatePositionAsync(Position position);
}

public interface ITradeLogger
{
    Task LogSignalAsync(TradeSignal signal);
    Task LogTradeExecutionAsync(string message);
}
