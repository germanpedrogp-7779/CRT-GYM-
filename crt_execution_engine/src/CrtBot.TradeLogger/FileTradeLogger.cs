using CrtBot.Core.Interfaces;
using System.Collections.Concurrent;

namespace CrtBot.TradeLogger;

public class FileTradeLogger : ITradeLogger
{
    private readonly string _logPath = "trades.log";
    private readonly object _lock = new();

    public void LogTrade(string message)
    {
        lock (_lock)
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            System.IO.File.AppendAllText(_logPath, logEntry);
        }
    }
}