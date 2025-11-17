# Agent Guidelines for AirWave

- Luôn trả lời bang tiếng Việt

## Build & Test Commands
- Build: `dotnet build` (solution level) or `dotnet build <ProjectName>/<ProjectName>.csproj`
- Run specific project: `cd <ProjectName> && dotnet run`
- Test: No test projects currently exist
- Restore: `dotnet restore`

## Project Structure
- **AirWave.Shared**: Shared models and helpers (.NET 9 class library)
- **AirWave.Sensor**: MQTT publisher console app
- **AirWave.Server**: Worker Service (MQTT subscriber + SQLite persistence)
- **AirWave.API**: ASP.NET Core Web API with Entity Framework Core
- **AirWave.Client**: Blazor Server dashboard

## Code Style
- **Target Framework**: .NET 9.0
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled - avoid redundant using statements
- **Namespaces**: File-scoped namespaces (e.g., `namespace AirWave.API.Controllers;`)
- **Naming**: PascalCase for public members, _camelCase for private fields
- **Types**: Use nullable reference types (`?`) appropriately; prefer `int`, `string` over aliases
- **Error Handling**: Use structured logging via `ILogger<T>`; return appropriate HTTP status codes in controllers
- **Database**: SQLite with EF Core; use async methods (`ToListAsync`, `FirstOrDefaultAsync`)
- **MQTT**: HiveMQ public broker (broker.hivemq.com:1883), topic: `airwave/aqi`
