@echo off
setlocal

@echo Build project and deploy to sprint12.sitefpo.com

rem run the build
call "%~dp0build.cmd" /nopause
if not "%errorlevel%"=="0" (
    echo Build failed, skipping deploy.
    pause
    exit /b 1
)

rem deploy using shared library
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Import-Module '%~dp0..\..\Contensive5\scripts\contensive-build.psm1' -Force; Invoke-ContensiveDeploy -SiteUrl 'https://sprint12.sitefpo.com/installCollection' -CollectionZip '%~dp0..\collections\aoImportWizard\aoImportWizard.zip' -SiteName 'sprint12'"
set deployExit=%errorlevel%

pause
exit /b %deployExit%
