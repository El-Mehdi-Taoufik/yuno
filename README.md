# Yuno Hub (independent rebuild)

An independent Windows desktop rebuild based on the supplied static-analysis notes and hand-reconstructed core. This project does not modify the original executable or bundle/install third-party kernel drivers.

## Current implementation
- Avalonia desktop shell with a dark dashboard.
- Editable mouse-processing settings: Ripple Control, Angle Snapping, Motion Sync, acceleration/deceleration, click brake, X/Y sensitivity and DPI scaling.
- JSON profile save/load and automatic current-profile persistence.
- Pure managed mouse-transform core (not connected to system input yet).
- GitHub Actions workflow to publish a self-contained Windows x64 build.

## Build locally
Install the .NET 8 SDK, then run:
```powershell
dotnet restore
dotnet publish src/YunoHub.Desktop/YunoHub.Desktop.csproj -c Release -r win-x64 --self-contained true
```

The published executable is written under `src/YunoHub.Desktop/bin/Release/net8.0/win-x64/publish`.

## Safety / scope
Input interception, Raw Input monitoring, DPI measurement, tray lifecycle, and Windows system tweaks are separate follow-up modules. The current build intentionally does not inject or intercept system-wide mouse input and does not change registry or boot settings. These need opt-in controls, restore paths, and runtime verification before being enabled.
