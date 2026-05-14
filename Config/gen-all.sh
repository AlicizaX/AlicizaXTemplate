#!/bin/bash
cd "$(dirname "$0")" || exit 1

if [[ -n "$1" ]]; then
    TEMPLATE="$1"
else
    INTERACTIVE=1
    echo "Select generation mode:"
    echo "  1. Normal (ClientTemplate)"
    echo "  2. Lazy   (ClientTemplateLazy)"
    read -rp "Enter 1 or 2: " CHOICE
    [[ "$CHOICE" == "2" ]] && TEMPLATE=ClientTemplateLazy || TEMPLATE=ClientTemplate
fi

dotnet ./CustomeTools/ConfigGenerate/ConfigGenerate.dll config.ini "$TEMPLATE"
EXIT_CODE=$?

if [[ -n "$INTERACTIVE" ]]; then
    read -rp "Press Enter to continue..."
fi

exit "$EXIT_CODE"
