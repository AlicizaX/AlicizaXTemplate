@echo off
setlocal enabledelayedexpansion
cd /d %~dp0

if defined LUBAN_TEMPLATE (
    set "TEMPLATE=%LUBAN_TEMPLATE%"
    goto template_selected
)

if not "%~1"=="" (
    set "TEMPLATE=%~1"
    goto template_selected
)

set "INTERACTIVE=1"
echo Select generation mode:
echo   1. Normal (ClientTemplate)
echo   2. Lazy   (ClientTemplateLazy)
set /p CHOICE="Enter 1 or 2: "
if "!CHOICE!"=="2" (set "TEMPLATE=ClientTemplateLazy") else (set "TEMPLATE=ClientTemplate")

:template_selected

dotnet ".\CustomeTools\ConfigGenerate\ConfigGenerate.dll" "config.ini" "!TEMPLATE!"
set "EXIT_CODE=!ERRORLEVEL!"

if defined INTERACTIVE pause
exit /b %EXIT_CODE%
