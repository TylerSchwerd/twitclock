[CmdletBinding()]
param(
    [ValidateSet("win-x64", "win-arm64", "win-x86")]
    [string]$RuntimeIdentifier = "win-x64",

    [ValidatePattern("^\d+\.\d+\.\d+$")]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "src\TwitClock\TwitClock.csproj"
$output = Join-Path $PSScriptRoot "artifacts\$RuntimeIdentifier"

$publishArguments = @(
    "publish"
    $project
    "--configuration"
    "Release"
    "--runtime"
    $RuntimeIdentifier
    "--self-contained"
    "true"
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "-p:DebugType=None"
    "-p:DebugSymbols=false"
    "--output"
    $output
)

if ($Version) {
    $publishArguments += @(
        "-p:Version=$Version"
        "-p:AssemblyVersion=$($Version).0"
        "-p:FileVersion=$($Version).0"
    )
}

& dotnet @publishArguments

if ($LASTEXITCODE -ne 0) {
    throw "TwitClock publishing failed with exit code $LASTEXITCODE."
}

Write-Host "TwitClock was published to $output" -ForegroundColor Green
