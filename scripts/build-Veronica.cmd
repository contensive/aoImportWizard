@echo off
setlocal

set appName=veronica
set collectionName=aoImportWizard
set deploymentRoot=C:\Deployments\aoImportWizard

@echo Build project and install on site: %appName%

rem run the build
call "%~dp0build.cmd" /nopause
if not "%errorlevel%"=="0" (
    echo Build failed, skipping install.
    pause
    exit /b %errorlevel%
)

rem find the latest versioned deployment folder (most recently created, name starts with digit)
for /f "delims=" %%d in ('dir /b /ad /od "%deploymentRoot%"') do (
    echo %%d | findstr /r "^[0-9]" >nul && set latestVersion=%%d
)
if not defined latestVersion (
    echo No deployment folder found.
    pause
    exit /b 1
)
echo Installing %collectionName%.zip from %deploymentRoot%\%latestVersion%
cd /d "%deploymentRoot%\%latestVersion%"
cc -a %appName% --installFile "%collectionName%.zip"
if errorlevel 1 (
    echo Failure installing.
    pause
    exit /b %errorlevel%
)
pause
