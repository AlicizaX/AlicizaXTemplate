#!/bin/bash
cd "$(dirname "$0")" || exit 1

# Compatibility entry:
# gen_code_bin.sh <Language> <TableDataOutPath> <CodeOutPath> <TemplateDir> [L10nEditorOut] [L10nTextOutPath] [L10nConstOutPath] [L10nKeyCodeOut] [L10nKeyCommentLanguage] [L10nConstOutRule]

if [[ -z "$4" ]]; then
    echo "Usage: gen_code_bin.sh <Language> <TableDataOutPath> <CodeOutPath> <TemplateDir> [L10nEditorOut] [L10nTextOutPath] [L10nConstOutPath] [L10nKeyCodeOut] [L10nKeyCommentLanguage] [L10nConstOutRule]" >&2
    exit 1
fi

LANGUAGE="$1"
TABLE_DATA_OUTPATH="$2"
CODE_OUTPATH="$3"
TEMPLATE_DIR="$4"
if [[ ! -e "$TEMPLATE_DIR" && -e "./CustomeTools/$TEMPLATE_DIR" ]]; then
    TEMPLATE_DIR="./CustomeTools/$TEMPLATE_DIR"
fi
LOCALIZATION_EDITOR_OUT="${5:-../Client/Assets/Editor/Config}"
LOCALIZATION_TEXT_OUTPATH="${6:-$TABLE_DATA_OUTPATH}"
LOCALIZATION_CONST_OUTPATH="${7:-$TABLE_DATA_OUTPATH}"
LOCALIZATION_KEY_CODE_OUT="${8:-../Client/Assets/Scripts/Hotfix/GameLogic/LocalizationKey.cs}"
LOCALIZATION_KEY_COMMENT_LANGUAGE="${9:-ChineseSimplified}"
LOCALIZATION_CONST_OUT_RULE="${10:-}"

dotnet "./Tools/Luban.dll" \
    -t client \
    -c cs-bin \
    -d bin \
    --conf "./luban.conf" \
    --customTemplateDir "$TEMPLATE_DIR" \
    -x outputDataDir="${TABLE_DATA_OUTPATH%/}/" \
    -x outputCodeDir="$CODE_OUTPATH" \
    -x l10n.provider=default \
    -x l10n.textFile.path="./Excels/Localization/Localization.xlsx" \
    -x l10n.textFile.keyFieldName=key \
    -x l10n.textListFile=texts.txt

if [[ $? -ne 0 ]]; then exit 1; fi

find "$CODE_OUTPATH" -name "*.cs" -print0 | while IFS= read -r -d '' file; do
    tmp="${file}.tmp"
    printf '\357\273\277' > "$tmp"
    sed '1s/^\xEF\xBB\xBF//' "$file" >> "$tmp"
    mv "$tmp" "$file"
done

dotnet ./CustomeTools/ConfigGenerate/ConfigGenerate.dll --export-localization-text ./Excels/Localization/Localization.xlsx "$LANGUAGE" "$LOCALIZATION_TEXT_OUTPATH" &&
dotnet ./CustomeTools/ConfigGenerate/ConfigGenerate.dll --export-localization-const ./Excels/Localization/LocalizationConst.xlsx "$LANGUAGE" "$LOCALIZATION_CONST_OUTPATH" "$LOCALIZATION_EDITOR_OUT" &&
dotnet ./CustomeTools/ConfigGenerate/ConfigGenerate.dll --export-localization-key ./Excels/Localization/LocalizationConst.xlsx "$LOCALIZATION_KEY_CODE_OUT" "$LOCALIZATION_KEY_COMMENT_LANGUAGE" "$LOCALIZATION_CONST_OUT_RULE"
