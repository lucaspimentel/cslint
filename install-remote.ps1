#!/usr/bin/env pwsh
#Requires -Version 7.0

[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Justification = 'Interactive installer script')]

<#
.SYNOPSIS
    Downloads and installs cslint from GitHub releases.

.DESCRIPTION
    This script downloads a pre-built cslint binary from GitHub releases
    and installs it to ~/.local/bin/cslint (or ~/.local/bin/cslint.exe on Windows).

    No build tools are required - only PowerShell 7.0+.

.PARAMETER Version
    The version to install. Defaults to "latest".
    Examples: "latest", "1.0.0"

.PARAMETER Force
    Skip confirmation prompts and overwrite existing installation.

.EXAMPLE
    ./install-remote.ps1

    Downloads and installs the latest version of cslint.

.EXAMPLE
    ./install-remote.ps1 -Version 1.0.0

    Downloads and installs version 1.0.0 of cslint.

.EXAMPLE
    irm https://raw.githubusercontent.com/lucaspimentel/Cslint/main/install-remote.ps1 | iex

    One-liner to download and run this installer script directly from GitHub.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version = 'latest',

    [Parameter()]
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$ProjectName = 'cslint'
$GitHubOwner = 'lucaspimentel'
$GitHubRepo = 'Cslint'

# Determine runtime identifier and archive extension
$rid = if ($IsWindows) {
    'win-x64'
} elseif ($IsLinux) {
    'linux-x64'
} else {
    Write-Host "Error: Unsupported platform. Only Windows (win-x64) and Linux (linux-x64) are supported." -ForegroundColor Red
    Write-Host "To build from source on other platforms, use: ./install-local.ps1" -ForegroundColor Cyan
    exit 1
}

$archiveExt = if ($IsWindows) { 'zip' } else { 'tar.gz' }
$exeName = if ($IsWindows) { "$ProjectName.exe" } else { $ProjectName }

Write-Host "Installing $ProjectName for $rid..." -ForegroundColor Cyan

$baseUrl = "https://github.com/$GitHubOwner/$GitHubRepo"

# Determine download URL and version
$downloadUrl = $null
$actualVersion = $null

try {
    if ($Version -eq 'latest') {
        Write-Host "Fetching latest release information from GitHub..." -ForegroundColor Cyan

        $apiUrl = "https://api.github.com/repos/$GitHubOwner/$GitHubRepo/releases/latest"
        $release = Invoke-RestMethod -Uri $apiUrl -ErrorAction Stop

        $actualVersion = $release.tag_name -replace '^v', ''
        $assetName = "$ProjectName-$rid.$archiveExt"

        $asset = $release.assets | Where-Object { $_.name -eq $assetName }
        if (-not $asset) {
            Write-Host "Error: Asset '$assetName' not found in latest release." -ForegroundColor Red
            Write-Host "Available assets: $($release.assets.name -join ', ')" -ForegroundColor Red
            exit 1
        }

        $downloadUrl = $asset.browser_download_url
        Write-Host "Latest version: $actualVersion" -ForegroundColor Cyan
    } else {
        $actualVersion = $Version
        $tag = "v$($Version.TrimStart('v'))"
        $assetName = "$ProjectName-$rid.$archiveExt"
        $downloadUrl = "$baseUrl/releases/download/$tag/$assetName"

        Write-Host "Version: $actualVersion" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Error: Failed to fetch release information: $_" -ForegroundColor Red
    exit 1
}

# Installation directory setup
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

# Create temporary directory for download
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "$ProjectName-install-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    # Download archive
    $archivePath = Join-Path $tempDir "$ProjectName-$rid.$archiveExt"
    Write-Host "Downloading from: $downloadUrl" -ForegroundColor Cyan

    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $archivePath -ErrorAction Stop
    } catch {
        Write-Host "Error: Failed to download: $_" -ForegroundColor Red
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "Release not found. Please verify the version exists at: $baseUrl/releases" -ForegroundColor Red
        }
        exit 1
    }

    Write-Host "Download complete!" -ForegroundColor Green

    # Extract archive
    $extractPath = Join-Path $tempDir 'extract'
    New-Item -ItemType Directory -Path $extractPath -Force | Out-Null

    Write-Host "Extracting archive..." -ForegroundColor Cyan
    if ($IsWindows) {
        Expand-Archive -Path $archivePath -DestinationPath $extractPath -Force
    } else {
        tar -xzf $archivePath -C $extractPath
        if ($LASTEXITCODE -ne 0) {
            throw "tar extraction failed with exit code $LASTEXITCODE"
        }
    }

    # Find and copy executable
    $exePath = Join-Path $extractPath $exeName

    if (-not (Test-Path $exePath)) {
        Write-Host "Error: Executable not found in archive at: $exePath" -ForegroundColor Red
        exit 1
    }

    # Copy to installation directory
    Write-Host "Installing to: $installPath" -ForegroundColor Cyan
    Copy-Item $exePath $installPath -Force

    # Set executable permissions on Unix systems
    if (-not $IsWindows) {
        chmod +x $installPath
    }

    Write-Host "`nInstallation complete! $ProjectName $actualVersion is now installed." -ForegroundColor Green

} catch {
    Write-Host "Error: Installation failed: $_" -ForegroundColor Red
    exit 1
} finally {
    # Cleanup temporary directory
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Display setup instructions
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
