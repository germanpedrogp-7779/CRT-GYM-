namespace CrtBot.Core.Models;

public record TradeSignal(
    string Ticker,
    string Action, // BUY, SELL, CLOSE_ALL
    decimal Price,
    decimal Sl,
    decimal Tp1,
    decimal Tp2,
    decimal Tp3,
    decimal Tp1Pct,
    decimal Tp2Pct,
    decimal Tp3Pct,
    string? Reason = null
);

public enum PositionDirection { Long, Short }

public class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ticker { get; set; } = string.Empty;
    public PositionDirection Direction { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal InitialQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal StopLoss { get; set; }
    public List<TakeProfitLevel> TakeProfitLevels { get; set; } = new();
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public bool IsClosed { get; set; }
}

public class TakeProfitLevel
{
    public decimal Price { get; set; }
    public decimal QuantityPercentage { get; set; }
    public bool IsHit { get; set; }
}
