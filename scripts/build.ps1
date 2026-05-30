#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\..\Contensive5\scripts\contensive-build.psm1') -Force

$projectRoot = (Resolve-Path "$PSScriptRoot\..").Path

Invoke-ContensiveBuild `
    -CollectionName    'aoImportWizard' `
    -CollectionPath    "$projectRoot\collections\aoImportWizard" `
    -SolutionPath      "$projectRoot\source\aoImportWizard\aoImportWizard.sln" `
    -BinPath           "$projectRoot\source\aoImportWizard\bin\Release\netstandard2.0" `
    -DeploymentRoot    'C:\Deployments\aoImportWizard' `
    -CleanFolders      @(
                           "$projectRoot\source\aoImportWizard\bin"
                           "$projectRoot\source\aoImportWizard\obj"
                       )
