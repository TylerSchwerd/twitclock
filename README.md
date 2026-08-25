# TwitClock for Windows

TwitClock is a small, always-on-top broadcast timer for Windows 10 and Windows 11. It alternates automatically between a 15-minute green **CONTENT** phase and a 90-second red **AD BREAK** phase.

This repository is a Windows-native rewrite of the original macOS SwiftUI program. The Windows version uses WPF and [.NET 10 LTS](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), which is supported through November 14, 2028; it does not attempt to run the macOS source through a compatibility layer.

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

## Download TwitClock

Versioned downloads are published on the [GitHub Releases](https://github.com/TylerSchwerd/twitclock/releases) page:

- `TwitClock-win-x64.exe` for most Intel and AMD Windows 10/11 computers
- `TwitClock-win-arm64.exe` for ARM-based Windows computers
- `TwitClock-win-x86.exe` for 32-bit Windows 10 computers
- `SHA256SUMS.txt` for verifying the downloaded files

Release downloads remain attached to their versioned GitHub Release instead of expiring with temporary GitHub Actions artifacts. Because the executables are self-contained, the target computer does not need a separate .NET installation.

The release workflow Authenticode signs and timestamps the executables when its signing-certificate secrets are configured. Until a trusted certificate is configured, releases remain unsigned and may trigger Microsoft Defender SmartScreen. Signing identifies the publisher, but SmartScreen reputation can still take time to develop.

## Build locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on Windows 10 or Windows 11, then run:

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

## Create a versioned release

Push a semantic-version tag to run the release workflow:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The workflow rebuilds and tests the tagged commit, publishes all three Windows executables, optionally signs them, creates SHA-256 checksums, and stages everything in a draft before publishing the GitHub Release. An existing semantic-version tag that does not yet have a Release can be published from **Actions → Release Windows executables → Run workflow**.

This workflow treats published releases as immutable and will not replace their downloads. If a release needs a correction, commit the fix and create a new patch-version tag such as `v1.0.1`. Repository administrators can additionally enable GitHub's release-immutability setting to prevent manual changes through the website or API.

### Configure code signing

Obtain a trusted Windows code-signing certificate as a password-protected PFX file. In the repository, create an Actions environment named `windows-code-signing`, configure required reviewers for it, and create these **environment secrets**:

- `WINDOWS_SIGNING_CERTIFICATE_BASE64`: Base64-encoded contents of the PFX file
- `WINDOWS_SIGNING_CERTIFICATE_PASSWORD`: the PFX password

On Windows, copy the Base64 value to the clipboard without exposing the certificate in the repository:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\TwitClock-signing.pfx")) | Set-Clipboard
```

The protected environment keeps the signing identity out of the untrusted build job and requires approval before a fresh runner receives it. Never commit the PFX file, its password, or its Base64 contents. When the environment secrets are absent, the workflow publishes an unsigned release and displays a warning.

## Project layout

| Path | Purpose |
| --- | --- |
| `src/TwitClock` | WPF user interface and Windows application manifest |
| `src/TwitClock.Core` | Platform-independent timer and phase logic |
| `tests/TwitClock.Core.Tests` | Dependency-free executable test harness |
| `tests/TwitClock.Ui.Tests` | Windows-only keyboard and background-animation behaviour tests |
| `build.ps1` | Local self-contained publishing script |
| `.github/workflows/build-windows.yml` | Automated Windows build, test, and artifact publishing |
| `.github/workflows/release-windows.yml` | Versioned release publishing and optional Authenticode signing |

## Licence

No licence file is currently included in this repository. The repository owner should confirm redistribution rights and add an appropriate licence before distributing modified builds publicly.
