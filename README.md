This repository contains a sample trading application implemented in .NET. The projects target .NET 10 (compatible with .NET 9+), and the solution implements an API, services, EF Core persistence, background processing using hosted services and channels, and a small rules engine.


### Working Application

- The solution is runnable with .NET 9 or higher (project targets .NET 10).
- Background processing of market ticks is implemented with hosted services and a `Channel<PriceUpdateDTO>` for asynchronous processing.
- Business logic lives in `Trading.Services` (`TradingRulesService`, `TradingProcessService`).
- Persistence uses EF Core with `TradingDbContext` implementing `ITradingDbContext`.


### How to Run

Prerequisites
- .NET SDK 9 or 10 installed (recommended: .NET 10).
- SQL Server instance (LocalDB, SQL Server Developer, or Dockerized SQL Server).
- Optional: Visual Studio 2026 (IDE instructions below).

Quick CLI steps
1. Clone the repository:
   - git clone https://github.com/mmavrodi/Trading C:\Projects\Trading
2. Configure the database:
   - Edit the connection string in `Trading.Api\appsettings.json` (or use user secrets / environment variable)
   - The configuration binding uses `ConfigurationSettings.DbConnectionString` (`Trading.Api\Configuations\ConfigurationSettings.cs`).
3. Apply EF Core migrations:
   - From repository root:
     - dotnet tool restore
     - dotnet ef database update --project Trading.DataAccess --startup-project Trading.Api
4. Run the API:
   - dotnet run --project Trading.Api
   - Or open `Trading.slnx` in Visual Studio and run the `Trading.Api` project (it will launch Swagger UI in Development).
5. Tests:
   - dotnet test

Notes
- Background services (`MarketDataSimulatorBackgroundService`, `PriceProcessorBackgroundService`) are registered in `Program.cs` and run automatically when the API starts.
- If you don't want to apply migrations to a real DB for quick testing, update the DI registration to use an in-memory DB in `Program.cs` (not included in mainline).

### Design Decisions

Overview
- Project separation:
  - `Trading.Api` — Web API and host (DI, configuration).
  - `Trading.Services` — Business logic, rules engine (`TradingRulesService`), processing (`TradingProcessService`).
  - `Trading.DataAccess` — EF Core `TradingDbContext`, entity configurations and migrations.
  - `Trading.Repository` — Configuration and simple repositories (rules repository is a thread-safe singleton).
  - `Trading.Cache` — In-memory thread-safe price cache (`IPriceCache`).
  - `Trading.Services.Tests` — Unit tests for service logic.

Component communication
- Dependency Injection (built-in Microsoft DI) wires services together in `Program.cs`.
- Background processing:
  - Market ticks are produced by `MarketDataSimulatorBackgroundService` and written into a `Channel<PriceUpdateDTO>`.
  - `PriceProcessorBackgroundService` reads the channel (configured `SingleReader = true`) and updates `IPriceCache` and invokes `TradingProcessService` work.
- Synchronous flows (e.g., manual order processing) call into services that use `ITradingDbContext` for persistence.

Persistence tradeoffs
- EF Core + SQL Server chosen for familiarity and productivity:
  - Pros: strong developer ergonomics, migrations support, clear LINQ querying (`TradingProcessService.GetTradeOrdersAsync`).
  - Cons: may not be optimal for very high-throughput low-latency trade storage. For ultra-high throughput, a specialized time-series store or native binary protocol might be preferable.
- The design uses a single `DbContext` per scope (scoped DI) for safety with async flows.

Concurrency & performance decisions
- `IPriceCache` and `TradingRulesRepository` are registered as singletons and implemented as thread-safe structures to support concurrent background and API access.
- `Channel<T>` with `SingleReader = true` optimizes the common pattern: one background processor consuming ticks, many producers.
- Auto-trading logic is in `TradingProcessService.EvaluateAndExecuteAutoTradeAsync` and is intentionally synchronous within the background processing loop to preserve ordering and simplicity.

Why these choices?
- Prioritize readability, testability, and safety over micro-optimizations.
- Hosted services + channels provide a simple, robust model for asynchronous market data ingestion without introducing external queue dependencies.

### AI Usage Transparency

Written by Engineer:
  - Architectural design (Channel-based decoupling, In-Memory Caching strategy, and DB persistence trade-offs).
  - Core domain entities, business logic definitions (auto-trading logic, math calculations), and dependency injection architecture.
  - System boundaries, interface segregations (`IPriceCache`, `ITradingRulesEngine`), and concurrency strategy.

AI Assistance (Copilot) Used For:
  - Generating standard boilerplate code.
  - Speeding up xUnit unit test structure generation using `FakeItEasy` and `FluentAssertions`.
  - Copilot ghost text


### Known Limitations & Future Improvements

Known limitations
- No authentication/authorization on API endpoints (security is not implemented).
- Error handling and structured logging are minimal; observability could be improved (metrics, tracing).
- Persistence model uses EF Core and SQL Server; might become a bottleneck at very high tick ingest rates.
- Business rules reside in in-memory singleton repo; dynamic distributed config updates are not supported.
- Limited or no rate-limiting, backpressure handling beyond the unbounded channel option.

Future improvements (if more time)
- Add robust logging (structured logs), metrics (Prometheus), and distributed tracing (OpenTelemetry).
- Add authentication, authorization, and role-based access controls for the API.
- Harden error handling and add retry/backoff strategies for DB operations.
- Replace `Channel` unbounded usage with bounded channels and apply backpressure or a fast persistent queue for extreme throughput.
- Split hot-path computations into dedicated worker pool for better CPU utilization and scale-out.
- Add more extensive integration tests and CI pipeline to run migrations and tests.
