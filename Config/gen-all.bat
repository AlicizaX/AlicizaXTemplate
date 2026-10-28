@echo off
setlocal
chcp 65001 >nul
cd /d "%~dp0"

set "CHECK="
if /I "%~1"=="--check" (
    set "CHECK=-x alicizax.check=true"
) else if not "%~1"=="" (
    echo Usage: gen-all.bat [--check] 1>&2
    exit /b 2
)

if not exist "Tools\Luban.AlicizaX.Localization.dll" (
    echo Luban localization extension is missing. Run build.bat. 1>&2
    exit /b 1
)

dotnet "Tools\Luban.dll" ^
    -t all ^
    --conf "luban.conf" ^
    -f ^
    -p alicizax-localization ^
    --strict ^
    -x "outputSaver=alicizax-stage" ^
    -x "alicizax.config=config.ini" ^
    -x "alicizax.validationOnly=true" ^
    -x "l10n.provider=alicizax"
if errorlevel 1 exit /b %errorlevel%

dotnet "Tools\Luban.dll" ^
    -t client ^
    --conf "luban.conf" ^
    -c cs-bin ^
    -d bin ^
    -p alicizax-localization ^
    --customTemplateDir "Defines\ClientTemplate" ^
    --strict ^
    -x "outputSaver=alicizax-stage" ^
    -x "alicizax.config=config.ini" ^
    -x "l10n.provider=alicizax" ^
    %CHECK%
exit /b %errorlevel%
