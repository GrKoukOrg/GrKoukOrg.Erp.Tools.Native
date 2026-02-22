# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

This is a .NET MAUI single-project solution targeting Android, iOS, macCatalyst, and Windows. Currently on the `upgrade-to-NET10` branch (net10.0 TFMs).

```bash
# Build entire solution
dotnet build GrKoukOrg.Erp.Tools.Native.sln

# Build for a specific platform
dotnet build GrKoukOrg.Erp.Tools.Native/GrKoukOrg.Erp.Tools.Native.csproj -f net10.0-maccatalyst
dotnet build GrKoukOrg.Erp.Tools.Native/GrKoukOrg.Erp.Tools.Native.csproj -f net10.0-android
dotnet build GrKoukOrg.Erp.Tools.Native/GrKoukOrg.Erp.Tools.Native.csproj -f net10.0-windows10.0.19041.0

# Run on macOS
dotnet run --project GrKoukOrg.Erp.Tools.Native/GrKoukOrg.Erp.Tools.Native.csproj -f net10.0-maccatalyst
```

No automated test project exists. The primary IDE is JetBrains Rider.

## Architecture

**MVVM with CommunityToolkit.Mvvm** — PageModels (view models) use `[ObservableProperty]`, `[RelayCommand]`, and `[NotifyCanExecuteChangedFor]` source generators. All inherit from `ObservableObject`.

**Shell navigation** — Routes registered via `AddTransientWithShellRoute<TPage, TPageModel>("routeName")` in `MauiProgram.cs`. Navigation uses `Shell.Current.GoToAsync` with query-string parameters. Bulk data transfer between pages uses `INavigationParameterService` (singleton).

**DI registration** — All wiring in `MauiProgram.cs`. Repositories are `AddScoped`, pages/page-models are `AddTransientWithShellRoute`, services mix of scoped and singleton.

**Data layer (Repository pattern, raw SQLite)** — Each entity has a dedicated repository class in `Data/` using `Microsoft.Data.Sqlite` (no EF Core/ORM). Repos use lazy table initialization (`Init()` called before each operation). Schema changes are handled manually (no migration framework). `LocalCostTrackingRepo` is the most complex, implementing weighted average cost recalculation.

**Two external API clients:**
- `BusinessServerHttpDataAccess` (`IBusinessServerDataAccess`) — unauthenticated HTTP to a local-network "Business" accounting server. Uses named `HttpClient` ("BusinessServerApi") configured from settings.
- `ApiService` — JWT-authenticated HTTP to the cloud ERP server. Handles token refresh automatically. Tokens stored via `Preferences`.

**Error handling** — `ModalErrorHandler` wraps errors in `Shell.DisplayAlert` behind a `SemaphoreSlim`. Fire-and-forget async uses `TaskUtilities.FireAndForgetSafeAsync`.

## Key Directories

- `Models/` — DTOs and domain models (31 files)
- `PageModels/` — MVVM view models (12 files)
- `Pages/` — XAML views and code-behinds (14 pages + `Controls/`)
- `Data/` — SQLite repository classes + `Constants.cs` (DB path)
- `Services/` — API clients, settings service, startup checker, seed data
- `Shared/` — Request/response DTOs for ERP API communication
- `Converters/` — XAML value converters
- `Behaviors/` — XAML behaviors

## UI Libraries

- **Syncfusion** — `SfDataGrid`, `SfListView`, `SfPullToRefresh`, `SfSegmentedControl`, `SfTextInputLayout`, `SfLinearProgressBar`
- **CommunityToolkit.Maui** — Snackbar/Toast, `EventToCommandBehavior`, popups
- **ZXing.Net.Maui** — Barcode scanning (partially implemented)
- **FluentUI** icon font — glyphs referenced via `FluentUI.cs` constants

## Runtime Configuration

Settings are stored via MAUI `Preferences` (not config files). Defaults:
- Business API: `http://192.168.20.38:1234`
- ERP API: `https://192.168.20.38:5001/`
- Company code: `NOTTOBAL`
- SQLite DB: `ErpToolsNativeSQLite.db3` in `Environment.SpecialFolder.LocalApplicationData`

## Conventions

- C# with nullable enabled and implicit usings
- Global usings in `GlobalUsings.cs`
- XAML pages and code-behinds use the namespace `GrKoukOrg.Erp.Tools.Native.Pages`
- PageModels use `GrKoukOrg.Erp.Tools.Native.PageModels`
- App ID: `com.grkoukorg.erp.tools-native`
