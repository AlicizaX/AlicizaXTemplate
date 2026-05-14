@echo off
setlocal enabledelayedexpansion
cd /d %~dp0

rem Compatibility entry:
rem gen_code_bin.bat <Language> <TableDataOutPath> <CodeOutPath> <TemplateDir> [L10nEditorOut] [L10nTextOutPath] [L10nConstOutPath] [L10nKeyCodeOut] [L10nKeyCommentLanguage] [L10nConstOutRule]

if "%~4"=="" (
    echo Usage: gen_code_bin.bat ^<Language^> ^<TableDataOutPath^> ^<CodeOutPath^> ^<TemplateDir^> [L10nEditorOut] [L10nTextOutPath] [L10nConstOutPath] [L10nKeyCodeOut] [L10nKeyCommentLanguage] [L10nConstOutRule]
    exit /b 1
)

set "LANGUAGE=%~1"
set "TABLE_DATA_OUTPATH=%~2"
set "CODE_OUTPATH=%~3"
set "TEMPLATE_DIR=%~4"
if not exist "%TEMPLATE_DIR%" (
    if exist ".\CustomeTools\%TEMPLATE_DIR%" set "TEMPLATE_DIR=.\CustomeTools\%TEMPLATE_DIR%"
)
if "%~5"=="" (set "LOCALIZATION_EDITOR_OUT=../Client/Assets/Editor/Config") else (set "LOCALIZATION_EDITOR_OUT=%~5")
if "%~6"=="" (set "LOCALIZATION_TEXT_OUTPATH=%TABLE_DATA_OUTPATH%") else (set "LOCALIZATION_TEXT_OUTPATH=%~6")
if "%~7"=="" (set "LOCALIZATION_CONST_OUTPATH=%TABLE_DATA_OUTPATH%") else (set "LOCALIZATION_CONST_OUTPATH=%~7")
if "%~8"=="" (set "LOCALIZATION_KEY_CODE_OUT=../Client/Assets/Scripts/Hotfix/GameLogic/LocalizationKey.cs") else (set "LOCALIZATION_KEY_CODE_OUT=%~8")
if "%~9"=="" (set "LOCALIZATION_KEY_COMMENT_LANGUAGE=ChineseSimplified") else (set "LOCALIZATION_KEY_COMMENT_LANGUAGE=%~9")
set "LOCALIZATION_CONST_OUT_RULE="
shift
shift
shift
shift
shift
shift
shift
shift
shift
if not "%~1"=="" set "LOCALIZATION_CONST_OUT_RULE=%~1"

dotnet ".\Tools\Luban.dll" ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf ".\luban.conf" ^
    --customTemplateDir "%TEMPLATE_DIR%" ^
    -x "outputDataDir=%TABLE_DATA_OUTPATH%" ^
    -x "outputCodeDir=%CODE_OUTPATH%" ^
    -x l10n.provider=default ^
    -x l10n.textFile.path="./Excels/Localization/Localization.xlsx" ^
    -x l10n.textFile.keyFieldName=key ^
    -x l10n.textListFile=texts.txt

if errorlevel 1 exit /b %ERRORLEVEL%

powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -LiteralPath '%CODE_OUTPATH%' -Recurse -Filter '*.cs' | ForEach-Object { $data = [System.IO.File]::ReadAllBytes($_.FullName); if ($data.Length -ge 3 -and $data[0] -eq 239 -and $data[1] -eq 187 -and $data[2] -eq 191) { $data = $data[3..($data.Length-1)] }; [System.IO.File]::WriteAllBytes($_.FullName, [byte[]](239,187,191) + $data) }"

dotnet ".\CustomeTools\ConfigGenerate\ConfigGenerate.dll" --export-localization-text ".\Excels\Localization\Localization.xlsx" "%LANGUAGE%" "%LOCALIZATION_TEXT_OUTPATH%"
if errorlevel 1 exit /b %ERRORLEVEL%
dotnet ".\CustomeTools\ConfigGenerate\ConfigGenerate.dll" --export-localization-const ".\Excels\Localization\LocalizationConst.xlsx" "%LANGUAGE%" "%LOCALIZATION_CONST_OUTPATH%" "%LOCALIZATION_EDITOR_OUT%"
if errorlevel 1 exit /b %ERRORLEVEL%
dotnet ".\CustomeTools\ConfigGenerate\ConfigGenerate.dll" --export-localization-key ".\Excels\Localization\LocalizationConst.xlsx" "%LOCALIZATION_KEY_CODE_OUT%" "%LOCALIZATION_KEY_COMMENT_LANGUAGE%" "%LOCALIZATION_CONST_OUT_RULE%"
exit /b %ERRORLEVEL%
