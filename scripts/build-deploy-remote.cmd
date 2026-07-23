@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "& '%~dp0build.ps1' -Configuration 'Debug' -DeployType 'remote'"
pause
exit /b %errorlevel%
