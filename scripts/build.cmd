@echo off
setlocal

set nopause=0
if /i "%~1"=="/nopause" set nopause=1

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
set exitcode=%errorlevel%

if not "%exitcode%"=="0" (
    echo.
    echo ========================================
    echo BUILD FAILED
    echo ========================================
)

rem if "%nopause%"=="0" pause
exit /b %exitcode%
