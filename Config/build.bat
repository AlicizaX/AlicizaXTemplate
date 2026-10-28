@echo off
setlocal EnableExtensions
set "CONFIG_DIR=%~dp0"
set "LUBAN_DIR=%CONFIG_DIR%Luban"
set "LUBAN_REPO=https://github.com/focus-creative-games/luban.git"
set "PLUGIN_DIR=%CONFIG_DIR%Extensions\Luban.AlicizaX.Localization"
set "BUILD_DIR=%CONFIG_DIR%Tools.build"
set "BACKUP_DIR=%CONFIG_DIR%Tools.backup"
set "TOOLS_DIR=%CONFIG_DIR%Tools"
set "SDK_DOWNLOAD_URL=https://dotnet.microsoft.com/download/dotnet/8.0"
set "NO_PAUSE="
if /I "%~1"=="--no-pause" set "NO_PAUSE=1"

call :Build
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" (
    echo [INFO] Luban build completed.
) else (
    echo [ERROR] Luban build stopped with exit code %RESULT%.
)
if not defined NO_PAUSE (
    echo.
    pause
)
exit /b %RESULT%

:Build
set "SDK_FOUND="
for /f "tokens=1 delims=." %%S in ('dotnet --list-sdks 2^>nul') do if %%S GEQ 8 set "SDK_FOUND=1"
if not defined SDK_FOUND goto :MissingSdk
dotnet --list-runtimes 2>nul | findstr /r /c:"^Microsoft.NETCore.App 8[.]" >nul
if errorlevel 1 goto :MissingRuntime
goto :SdkReady

:MissingSdk
echo [ERROR] No compatible .NET SDK is available to this script.
echo [ERROR] Required: .NET 8 SDK (8.0.x or newer), not just the Runtime.
echo [INFO] If already installed, add its dotnet directory to PATH and reopen this window.
goto :SdkDownload

:MissingRuntime
echo [ERROR] .NET 8 Runtime is missing. The .NET 8 SDK includes it.

:SdkDownload
echo [INFO] Download the .NET 8 SDK: %SDK_DOWNLOAD_URL%
echo [INFO] Existing Tools are unchanged.
if not defined NO_PAUSE start "" "%SDK_DOWNLOAD_URL%"
exit /b 1

:SdkReady
if exist "%LUBAN_DIR%" (
    call :CheckLubanRepo
    if errorlevel 1 (
        echo [ERROR] "%LUBAN_DIR%" is not a clean checkout of the official Luban repository. It was not changed.
        exit /b 1
    )
)

if exist "%BACKUP_DIR%" (
    echo [ERROR] %BACKUP_DIR% already exists. Check the previous Tools backup first.
    exit /b 1
)

set "LUBAN_REF="
for /f "usebackq delims=" %%R in (`powershell.exe -NoProfile -Command "$ErrorActionPreference='Stop'; $response=Invoke-WebRequest -UseBasicParsing -Uri 'https://github.com/focus-creative-games/luban/releases/latest'; $uri=$response.BaseResponse.ResponseUri; if (-not $uri) { $uri=$response.BaseResponse.RequestMessage.RequestUri }; if ($uri.AbsolutePath -notmatch '/releases/tag/([A-Za-z0-9][A-Za-z0-9._-]*)$') { exit 1 }; $Matches[1]"`) do set "LUBAN_REF=%%R"
if not defined LUBAN_REF (
    echo [ERROR] Could not resolve the latest Luban release. Existing Tools are unchanged.
    exit /b 1
)

if exist "%LUBAN_DIR%" (
    echo [INFO] Selecting Luban %LUBAN_REF% in "%LUBAN_DIR%"...
    git -C "%LUBAN_DIR%" fetch --tags origin "%LUBAN_REF%"
    if errorlevel 1 exit /b 1
    git -C "%LUBAN_DIR%" switch --detach "%LUBAN_REF%"
) else (
    echo [INFO] Cloning Luban %LUBAN_REF% into "%LUBAN_DIR%"...
    git clone --depth 1 --branch "%LUBAN_REF%" "%LUBAN_REPO%" "%LUBAN_DIR%"
)
if errorlevel 1 (
    echo [ERROR] Could not update Luban source in "%LUBAN_DIR%". Existing Tools are unchanged.
    exit /b 1
)

if exist "%BUILD_DIR%" rd /s /q "%BUILD_DIR%"
if exist "%BUILD_DIR%" (
    echo [ERROR] Could not clear staging directory "%BUILD_DIR%". Existing Tools are unchanged.
    exit /b 1
)

dotnet build "%LUBAN_DIR%\src\Luban\Luban.csproj" -c Release -o "%BUILD_DIR%" "-p:CustomAfterMicrosoftCommonTargets=%PLUGIN_DIR%\LubanHost.targets" "-p:AlicizaXLocalizationProject=%PLUGIN_DIR%\Luban.AlicizaX.Localization.csproj" "-p:LubanCoreProject=%LUBAN_DIR%\src\Luban.Core\Luban.Core.csproj"
if errorlevel 1 (
    echo [ERROR] Luban source build failed. Existing Tools are unchanged.
    exit /b 1
)
if not exist "%BUILD_DIR%\Luban.dll" (
    echo [ERROR] Build output is missing "%BUILD_DIR%\Luban.dll".
    exit /b 1
)
if not exist "%BUILD_DIR%\Luban.runtimeconfig.json" (
    echo [ERROR] Build output is missing "%BUILD_DIR%\Luban.runtimeconfig.json".
    exit /b 1
)
if not exist "%BUILD_DIR%\Luban.AlicizaX.Localization.dll" (
    echo [ERROR] Build output is missing the localization extension.
    exit /b 1
)
if not exist "%BUILD_DIR%\Templates\cs-bin\bean.sbn" (
    echo [ERROR] Build output is missing "%BUILD_DIR%\Templates\cs-bin\bean.sbn".
    exit /b 1
)

pushd "%CONFIG_DIR%"
dotnet "%BUILD_DIR%\Luban.dll" -t all --conf luban.conf -f -p alicizax-localization --strict -x outputSaver=alicizax-stage -x alicizax.config=config.ini -x alicizax.validationOnly=true -x l10n.provider=alicizax
if errorlevel 1 (
    popd
    echo [ERROR] Staged Luban validation failed. Existing Tools are unchanged.
    exit /b 1
)
dotnet "%BUILD_DIR%\Luban.dll" -t client --conf luban.conf -c cs-bin -d bin -p alicizax-localization --customTemplateDir Defines\ClientTemplate --strict -x outputSaver=alicizax-stage -x alicizax.config=config.ini -x l10n.provider=alicizax -x alicizax.check=true
if errorlevel 1 (
    popd
    echo [ERROR] Staged Luban output differs from generated files. Existing Tools are unchanged.
    exit /b 1
)
popd

move "%TOOLS_DIR%" "%BACKUP_DIR%" >nul
if errorlevel 1 (
    echo [ERROR] Could not back up existing Tools from "%TOOLS_DIR%".
    exit /b 1
)
move "%BUILD_DIR%" "%TOOLS_DIR%" >nul
if errorlevel 1 (
    echo [ERROR] Could not replace Tools. Restoring the previous build...
    move "%BACKUP_DIR%" "%TOOLS_DIR%" >nul
    if errorlevel 1 echo [ERROR] Restore failed. Previous Tools remain at "%BACKUP_DIR%".
    exit /b 1
)

call "%CONFIG_DIR%gen-all.bat" --check
if errorlevel 1 (
    echo [ERROR] Installed Tools failed verification. Restoring the previous build...
    move "%TOOLS_DIR%" "%BUILD_DIR%" >nul
    move "%BACKUP_DIR%" "%TOOLS_DIR%" >nul
    exit /b 1
)

rd /s /q "%BACKUP_DIR%"
if exist "%BACKUP_DIR%" (
    echo [ERROR] New Tools were installed, but old backup remains at "%BACKUP_DIR%".
    exit /b 1
)
call :CheckLubanRepo
if errorlevel 1 (
    echo [ERROR] Luban source changed during the build, so "%LUBAN_DIR%" was kept for safety.
    exit /b 1
)
rd /s /q "%LUBAN_DIR%"
if exist "%LUBAN_DIR%" (
    echo [ERROR] Tools were installed, but could not remove Luban source at "%LUBAN_DIR%".
    exit /b 1
)
echo [INFO] Luban was updated and installed in %TOOLS_DIR%.
exit /b 0

:CheckLubanRepo
powershell.exe -NoProfile -NonInteractive -Command "$expected=[IO.Path]::GetFullPath($env:LUBAN_DIR); $config=[IO.Path]::GetFullPath($env:CONFIG_DIR); if ($expected -ne [IO.Path]::Combine($config,'Luban') -or -not $expected.StartsWith($config,[StringComparison]::OrdinalIgnoreCase)) { exit 1 }; $item=Get-Item -LiteralPath $expected -Force -ErrorAction SilentlyContinue; if (-not $item -or (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)) { exit 1 }; $root=git -C $expected rev-parse --show-toplevel 2>$null; if ($LASTEXITCODE -ne 0 -or [IO.Path]::GetFullPath($root) -ne $expected) { exit 1 }; $remote=git -C $expected remote get-url origin 2>$null; if ($LASTEXITCODE -ne 0 -or $remote -ne $env:LUBAN_REPO) { exit 1 }; $status=git -C $expected status --porcelain --untracked-files=all 2>$null; if ($LASTEXITCODE -ne 0 -or $status) { exit 1 }"
exit /b %ERRORLEVEL%
