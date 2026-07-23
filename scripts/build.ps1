#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]   $Configuration      = 'Release',
    [ValidateSet('','local','remote')]
    [string]   $DeployType         = ''
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\..\Contensive5\scripts\build-addon-collection.psm1') -Force

$projectRoot = (Resolve-Path "$PSScriptRoot\..").Path

# Display build info
$version = Get-ContensiveVersion -DeploymentRoot 'C:\Deployments\aoImportWizard'
Write-Host ""
Write-Host "========================================"
Write-Host "  aoImportWizard Build"
Write-Host "  Version:       $version"
Write-Host "  Configuration: $Configuration"
Write-Host "========================================"
Write-Host ""

# After displaying info, either prompt for deploy target or pause
$LocalDeployTarget  = ''
$RemoteDeployTarget = $null

switch ($DeployType) {
    'local' {
        $defaultsFile = Join-Path $env:LOCALAPPDATA 'contensive\local-deploy-appname.txt'
        $savedApp = ''
        if (Test-Path $defaultsFile) {
            $savedApp = (Get-Content $defaultsFile -First 1).Trim()
        }
        $prompt = if ($savedApp) { "Enter local site name [$savedApp]: " } else { "Enter local site name: " }
        $appName = Read-Host $prompt
        if (-not $appName) { $appName = $savedApp }
        if (-not $appName) {
            Write-Host "No site name entered."
            exit 1
        }
        $defaultsDir = Join-Path $env:LOCALAPPDATA 'contensive'
        if (-not (Test-Path $defaultsDir)) { New-Item -ItemType Directory -Path $defaultsDir | Out-Null }
        $appName | Set-Content $defaultsFile
        $LocalDeployTarget = $appName
        Write-Host "Deploy to: $appName (local)"
        Write-Host ""
    }
    'remote' {
        $defaultsFile = Join-Path $env:LOCALAPPDATA 'contensive\remote-deploy-domain.txt'
        $savedDomain = ''
        if (Test-Path $defaultsFile) {
            $savedDomain = (Get-Content $defaultsFile -First 1).Trim()
        }
        $prompt = if ($savedDomain) { "Enter remote domain [$savedDomain]: " } else { "Enter remote domain: " }
        $domain = Read-Host $prompt
        if (-not $domain) { $domain = $savedDomain }
        if (-not $domain) {
            Write-Host "No domain entered."
            exit 1
        }
        $defaultsDir = Join-Path $env:LOCALAPPDATA 'contensive'
        if (-not (Test-Path $defaultsDir)) { New-Item -ItemType Directory -Path $defaultsDir | Out-Null }
        $domain | Set-Content $defaultsFile
        $siteName = ($domain -split '\.')[0]
        $RemoteDeployTarget = @{ Url = "https://$domain/installCollection"; SiteName = $siteName }
        Write-Host "Deploy to: $domain (remote)"
        Write-Host ""
    }
    default {
        Write-Host "Press any key to continue or Ctrl+C to cancel..."
        $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
        Write-Host ""
    }
}

Invoke-ContensiveBuild `
    -CollectionName    'aoImportWizard' `
    -CollectionPath    "$projectRoot\collections\aoImportWizard" `
    -SolutionPath      "$projectRoot\source\aoImportWizard\aoImportWizard.sln" `
    -BinPath           "$projectRoot\source\aoImportWizard\bin\$Configuration\netstandard2.0" `
    -Configuration     $Configuration `
    -DeploymentRoot    'C:\Deployments\aoImportWizard' `
    -CleanFolders      @(
                           "$projectRoot\source\aoImportWizard\bin"
                           "$projectRoot\source\aoImportWizard\obj"
                       ) `
    -UiPath            "$projectRoot\ui" `
    -LocalDeployTarget  $LocalDeployTarget `
    -RemoteDeployTarget $RemoteDeployTarget
