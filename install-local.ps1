#!/usr/bin/env pwsh
#Requires -Version 7.0

[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Justification = 'Interactive installer script')]

<#
.SYNOPSIS
    Builds and installs cslint from source.

.DESCRIPTION
    This script builds cslint from source and installs it
    to ~/.local/bin/cslint (or ~/.local/bin/cslint.exe on Windows).

    Requirements:
    - PowerShell 7.0+
    - .NET 10 SDK

.PARAMETER Force
    Skip confirmation prompts and overwrite existing installation.

.PARAMETER Update
    Pull the latest changes from the remote before building.

.EXAMPLE
    ./install-local.ps1

    Builds and installs cslint to ~/.local/bin

.EXAMPLE
    ./install-local.ps1 -Force

    Builds and installs, overwriting any existing installation.

.EXAMPLE
    ./install-local.ps1 -Update -Force

    Pull latest changes, then build and install.
#>

[CmdletBinding()]
param(
    [switch]$Force,

    [switch]$Update
)

$ErrorActionPreference = 'Stop'

$ProjectName = 'cslint'
$ProjectFile = 'src/Cslint.Cli/Cslint.Cli.csproj'

# Check if the local clone is up-to-date with remote
Write-Host "Checking if repository is up-to-date..." -ForegroundColor Cyan
try {
    Push-Location $PSScriptRoot
    $branch = & git rev-parse --abbrev-ref HEAD 2>&1
    & git fetch origin $branch --quiet 2>&1
    $local = & git rev-parse HEAD 2>&1
    $remote = & git rev-parse "origin/$branch" 2>&1

    if ($local -ne $remote) {
        $behind = & git rev-list --count "HEAD..origin/$branch" 2>&1
        $ahead = & git rev-list --count "origin/$branch..HEAD" 2>&1
        $status = @()
        if ([int]$behind -gt 0) { $status += "$behind commit(s) behind" }
        if ([int]$ahead -gt 0) { $status += "$ahead commit(s) ahead of" }
        Write-Host "Warning: Local branch '$branch' is $($status -join ' and ') origin/$branch." -ForegroundColor Yellow

        if ($Update) {
            Write-Host "Pulling latest changes..." -ForegroundColor Cyan
            & git pull --quiet 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Error: git pull failed. Resolve conflicts and try again." -ForegroundColor Red
                exit 1
            }
            Write-Host "Repository updated successfully." -ForegroundColor Cyan
        } elseif (-not $Force) {
            $response = Read-Host "Continue anyway? (y/N)"
            if ($response -notmatch '^[Yy]') {
                Write-Host "Installation cancelled. Run 'git pull' or use -Update to update." -ForegroundColor Cyan
                exit 0
            }
        }
    } else {
        Write-Host "Repository is up-to-date with origin/$branch." -ForegroundColor Cyan
    }
} catch {
    Write-Host "Warning: Could not check remote status: $_" -ForegroundColor Yellow
    Write-Host "Continuing with installation..." -ForegroundColor Yellow
} finally {
    Pop-Location
}

# Determine runtime identifier based on platform
$rid = if ($IsWindows) {
    'win-x64'
} elseif ($IsMacOS) {
    if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq 'Arm64') {
        'osx-arm64'
    } else {
        'osx-x64'
    }
} elseif ($IsLinux) {
    'linux-x64'
} else {
    throw "Unsupported platform"
}

$exeName = if ($IsWindows) { "$ProjectName.exe" } else { $ProjectName }

Write-Host "Building $ProjectName for $rid..." -ForegroundColor Cyan

# Check for .NET SDK
try {
    $dotnetVersion = & dotnet --version 2>&1
    Write-Host ".NET SDK version: $dotnetVersion" -ForegroundColor Cyan
} catch {
    Write-Host "Error: .NET SDK not found. Please install .NET 10 SDK or later." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Cyan
    exit 1
}

# Build and publish the project
# With UseArtifactsOutput, output lands at artifacts/publish/<ProjectName>/release_<rid>/
$projectPath = Join-Path $PSScriptRoot $ProjectFile
$publishPath = Join-Path $PSScriptRoot "artifacts/publish/Cslint.Cli/release_$rid"

try {
    dotnet publish $projectPath `
        -c Release `
        -r $rid

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host "Error: Failed to build project: $_" -ForegroundColor Red
    exit 1
}

# Verify executable exists
$exePath = Join-Path $publishPath $exeName

if (-not (Test-Path $exePath)) {
    Write-Host "Error: Build succeeded but executable not found at: $exePath" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Install to ~/.local/bin
$installDir = Join-Path $HOME '.local/bin'
$installPath = Join-Path $installDir $exeName

# Create installation directory if it doesn't exist
if (-not (Test-Path $installDir)) {
    Write-Host "Creating installation directory: $installDir" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Check for existing installation
if (Test-Path $installPath) {
    if (-not $Force) {
        Write-Host "$ProjectName is already installed at: $installPath" -ForegroundColor Yellow
        $response = Read-Host "Overwrite existing installation? (y/N)"
        if ($response -notmatch '^[Yy]') {
            Write-Host "Installation cancelled." -ForegroundColor Cyan
            exit 0
        }
    }
    Write-Host "Removing existing installation..." -ForegroundColor Yellow
    Remove-Item $installPath -Force
}

# Copy executable to installation directory
Write-Host "Installing to: $installPath" -ForegroundColor Cyan
Copy-Item $exePath $installPath -Force

# Set executable permissions on Unix systems
if (-not $IsWindows) {
    chmod +x $installPath
}

Write-Host "`nInstallation complete!" -ForegroundColor Green
Write-Host "`nTo use $ProjectName, ensure ~/.local/bin is in your PATH:" -ForegroundColor Cyan

if ($IsWindows) {
    Write-Host @"

Add to your PowerShell profile ($PROFILE):
    `$env:PATH = "`$HOME\.local\bin;`$env:PATH"

Then restart your shell or run:
    . `$PROFILE

"@ -ForegroundColor White
} else {
    Write-Host @"

Add to your shell's rc file (~/.bashrc, ~/.zshrc, etc.):
    export PATH="`$HOME/.local/bin:`$PATH"

Then restart your shell or source the file.

"@ -ForegroundColor White
}
