# TwitClock for Windows

TwitClock is a small, always-on-top broadcast timer for Windows 10 and Windows 11. It alternates automatically between a 15-minute green **CONTENT** phase and a 90-second red **AD BREAK** phase.

This repository is a Windows-native rewrite of the original macOS SwiftUI program. The Windows version uses WPF and .NET 8; it does not attempt to run the macOS source through a compatibility layer.

## Controls

| On-screen control | Action |
| --- | --- |
| `+` | Add one minute to the current phase |
| `−` | Subtract one minute, stopping at `00:00` |
| `⇄` | Switch immediately between CONTENT and AD BREAK |
| `×` | Close TwitClock |
| Drag the coloured background | Move the borderless window |

### Keyboard shortcuts

| Key | Action |
| --- | --- |
| `=`, `+`, numeric-keypad `+`, or `↑` | Add one minute to the current phase |
| `−`, numeric-keypad `−`, or `↓` | Subtract one minute, stopping at `00:00` |
| `←` or `→` | Switch immediately between CONTENT and AD BREAK |
| `X` | Close TwitClock |

Keyboard shortcuts work whenever TwitClock is the active window.

The window stays above normal windows and changes colour with a smooth half-second transition when the phase changes.

## Download a Windows executable

The **Build Windows executables** GitHub Actions workflow creates three self-contained artifacts:

- `TwitClock-win-x64` for most Intel and AMD Windows 10/11 computers
- `TwitClock-win-arm64` for ARM-based Windows computers
- `TwitClock-win-x86` for 32-bit Windows 10 computers

Each artifact contains a single `TwitClock.exe`. Because the builds are self-contained, the target computer does not need a separate .NET installation.

Unsigned executables downloaded from the internet can produce a Microsoft Defender SmartScreen warning. Code-signing the executable is the normal way to establish publisher reputation for wider distribution.

## Build locally

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows 10 or Windows 11, then run:

```powershell
dotnet build TwitClock.sln --configuration Release
dotnet run --project tests\TwitClock.Core.Tests\TwitClock.Core.Tests.csproj --configuration Release
dotnet run --project tests\TwitClock.Ui.Tests\TwitClock.Ui.Tests.csproj --configuration Release
dotnet run --project src\TwitClock\TwitClock.csproj --configuration Release
```

To create a self-contained single-file executable for a particular processor architecture:

```powershell
.\build.ps1 -RuntimeIdentifier win-x64
```

Valid runtime identifiers are `win-x64`, `win-arm64`, and `win-x86`. The finished executable is written beneath the `artifacts` directory.

## Project layout

| Path | Purpose |
| --- | --- |
| `src/TwitClock` | WPF user interface and Windows application manifest |
| `src/TwitClock.Core` | Platform-independent timer and phase logic |
| `tests/TwitClock.Core.Tests` | Dependency-free executable test harness |
| `tests/TwitClock.Ui.Tests` | Windows-only keyboard and background-animation behaviour tests |
| `build.ps1` | Local self-contained publishing script |
| `.github/workflows/build-windows.yml` | Automated Windows build, test, and artifact publishing |

## Licence

No licence file is currently included in this repository. The repository owner should confirm redistribution rights and add an appropriate licence before distributing modified builds publicly.
