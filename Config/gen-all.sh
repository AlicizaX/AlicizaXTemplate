#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"
if [[ "${1:-}" == "--check" ]]; then
    set -- -x alicizax.check=true
elif [[ $# -ne 0 ]]; then
    echo 'Usage: gen-all.sh [--check]' >&2
    exit 2
else
    set --
fi

if [[ ! -f Tools/Luban.AlicizaX.Localization.dll ]]; then
    echo 'Luban localization extension is missing. Rebuild Config/Tools.' >&2
    exit 1
fi

dotnet Tools/Luban.dll \
    -t all \
    --conf luban.conf \
    -f \
    -p alicizax-localization \
    --strict \
    -x outputSaver=alicizax-stage \
    -x alicizax.config=config.ini \
    -x alicizax.validationOnly=true \
    -x l10n.provider=alicizax

dotnet Tools/Luban.dll \
    -t client \
    --conf luban.conf \
    -c cs-bin \
    -d bin \
    -p alicizax-localization \
    --customTemplateDir Defines/ClientTemplate \
    --strict \
    -x outputSaver=alicizax-stage \
    -x alicizax.config=config.ini \
    -x l10n.provider=alicizax \
    "$@"
