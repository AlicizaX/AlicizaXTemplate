Cd /d %~dp0
echo %CD%

set WORKSPACE=../..
set LUBAN_DLL=./Tools/Luban.dll
set CONF_ROOT=.
set DATA_OUTPATH=%WORKSPACE%/Client/Assets/Bundles/Configs/bytes/ChineseSimplified/
set CODE_OUTPATH=%WORKSPACE%/Client/Assets/Scripts/Hotfix/GameProto/Config/Generate

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin^
    --conf %CONF_ROOT%\luban.conf ^
    --customTemplateDir "ClientTemplate" ^
    -x outputDataDir=%DATA_OUTPATH% ^
    -x outputCodeDir=%CODE_OUTPATH% ^
    -x l10n.provider=default ^
    -x l10n.textFile.path="./Excels/Localization/Localization.xlsx" ^
    -x l10n.textFile.keyFieldName=key ^
    -x l10n.textFile.languageFieldName=ChineseSimplified ^
    -x l10n.textListFile=texts.txt ^
    -x l10n.convertTextKeyToValue=1

pause