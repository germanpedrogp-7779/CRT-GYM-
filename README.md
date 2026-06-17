# CRT Bot — Candle Range Theory Trading Bot

Bot de trading automatizado que replica la lógica **Candle Range Theory (CRT)** con gestión de riesgo completa.

## Componentes

### 🟢 PineScript v6 (TradingView)
Estrategia CRT lista para usar en TradingView con:
- 3 modos de rango de referencia (Previous Bar, HTF Bar, Session Range)
- Detección de sweeps de liquidez con Close Rejection y Structure Break
- 3 niveles de take profit parcial con % configurables
- Stop loss dinámico (breakeven tras TP1, trailing stop)
- Cierre automático de operaciones los viernes a las 15:00 UTC-4
- Dashboard visual en pantalla
- Webhooks JSON listos para el backend C#

### 🔵 Motor C# (.NET 8)
Backend de ejecución con:
- `POST /webhook` — recibe señales de TradingView con verificación por secret
- `MockBrokerService` — simulación de ejecución de órdenes (paper trading)
- `PositionTrackerService` — seguimiento de posiciones con soporte de parciales
- `MarketMonitor` — monitorea SL/TP cada 5s, mueve SL a breakeven tras TP1
- `FridayCloseScheduler` — cierra todas las posiciones los viernes 15:00 UTC-4
- `RiskManagerService` — límite de pérdida diaria configurable
- `FileTradeLogger` — logging persistente a archivo

## Cómo empezar

### PineScript
1. Abre TradingView
2. Crea un nuevo indicador/estrategia en Pine Editor
3. Copia el contenido de `crt_bot_strategy.pine`
4. Compila y agrega al gráfico
5. Configura los parámetros desde el menú de ajustes

### Motor C#
```bash
cd crt_execution_engine
dotnet build
dotnet run --project src/CrtBot.WebhookReceiver --urls http://0.0.0.0:3000
```

### Webhook Payloads

**BUY:**
```json
{"ticker":"BTCUSD","action":"BUY","price":50000.0,"sl":49500.0,"tp1":50500.0,"tp1_pct":50,"tp2":51000.0,"tp2_pct":30,"tp3":51500.0,"tp3_pct":20,"secret":"CRT_BOT_2026"}
```

**SELL:**
```json
{"ticker":"BTCUSD","action":"SELL","price":50000.0,"sl":50500.0,"tp1":49500.0,"tp1_pct":50,"tp2":49000.0,"tp2_pct":30,"tp3":48500.0,"tp3_pct":20,"secret":"CRT_BOT_2026"}
```

**CLOSE_ALL:**
```json
{"ticker":"BTCUSD","action":"CLOSE_ALL","reason":"Friday Auto-Close","secret":"CRT_BOT_2026"}
```

## Endpoints del motor C#
- `POST /webhook` — Recibir alertas de TradingView
- `GET /positions` — Ver posiciones abiertas
- `GET /` — Health check

## Licencia
Código abierto para la comunidad. CRT Bot Solutions.