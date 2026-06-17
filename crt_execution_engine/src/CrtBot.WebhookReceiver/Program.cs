using CrtBot.Broker;
using CrtBot.Core.Interfaces;
using CrtBot.Core.Models;
using CrtBot.RiskManager;
using CrtBot.Scheduler;
using CrtBot.TradeLogger;
using CrtBot.WebhookReceiver.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<ITradeLogger, FileTradeLogger>();
builder.Services.AddSingleton<IBrokerService, MockBrokerService>();
builder.Services.AddSingleton<IRiskManager, RiskManagerService>();
builder.Services.AddSingleton<IPositionTracker, PositionTrackerService>();
builder.Services.AddSingleton<TradeService>();

// Bind RiskConfig from appsettings.json section "Risk"
builder.Services.Configure<RiskConfig>(builder.Configuration.GetSection("Risk"));

builder.Services.AddHostedService<MarketMonitor>();
builder.Services.AddHostedService<FridayCloseScheduler>();

var app = builder.Build();

app.MapPost("/webhook", async (
    [FromBody] TradingViewWebhook webhook,
    [FromServices] TradeService tradeService,
    [FromServices] IConfiguration config,
    ILogger<Program> logger) =>
{
    var secret = config["WebhookSecret"];
    if (!string.IsNullOrEmpty(secret) && webhook.Secret != secret)
    {
        logger.LogWarning("Unauthorized webhook request.");
        return Results.Unauthorized();
    }

    if (string.IsNullOrEmpty(webhook.Action) || string.IsNullOrEmpty(webhook.Ticker))
    {
        return Results.BadRequest("Invalid payload.");
    }

    _ = Task.Run(async () =>
    {
        try
        {
            await tradeService.HandleWebhookAsync(webhook);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling webhook.");
        }
    });

    return Results.Ok(new { status = "success" });
});

app.MapGet("/positions", (IPositionTracker tracker) =>
{
    return Results.Ok(tracker.GetOpenPositions());
});

app.MapGet("/", () => "CRT Execution Engine is active.");

app.Run();