#!/usr/bin/env bash
set -euo pipefail

fail() {
    printf '[ERROR] %s\n' "$1" >&2
    exit 1
}

[[ $# -eq 0 ]] || fail 'Usage: build.sh'

config_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
luban_dir="$config_dir/Luban"
build_dir="$config_dir/Tools.build"
backup_dir="$config_dir/Tools.backup"
tools_dir="$config_dir/Tools"
luban_repo='https://github.com/focus-creative-games/luban.git'
sdk_download_url='https://dotnet.microsoft.com/download/dotnet/8.0'

sdk_required() {
    printf '[ERROR] %s\n' "$1" >&2
    printf '[INFO] Building Luban requires .NET 8 SDK (8.0.x or newer) and the .NET 8 Runtime.\n' >&2
    printf '[INFO] Download the SDK (not just the Runtime): %s\n' "$sdk_download_url" >&2
    case "$(uname -s)" in
        Darwin*)
            if command -v open >/dev/null; then open "$sdk_download_url" >/dev/null 2>&1 || true; fi
            ;;
        MINGW*|MSYS*|CYGWIN*)
            if command -v cmd.exe >/dev/null; then cmd.exe /c start "" "$sdk_download_url" >/dev/null 2>&1 || true; fi
            ;;
    esac
    exit 1
}

verify_luban_checkout() {
    [[ "$luban_dir" == "$config_dir/Luban" && "$luban_dir" != "$config_dir" ]] \
        || fail 'Unsafe Luban source path.'
    [[ -d "$luban_dir/.git" && ! -L "$luban_dir" && ! -L "$luban_dir/.git" ]] \
        || fail "$luban_dir is not a regular Luban checkout. Existing files are unchanged."
    local resolved_dir origin changes
    resolved_dir=$(cd "$luban_dir" && pwd -P) || fail 'Could not resolve the Luban source path.'
    [[ "$resolved_dir" == "$luban_dir" ]] || fail 'Luban source path points outside Config.'
    origin=$(git -C "$luban_dir" remote get-url origin) || fail 'Could not identify Luban source origin.'
    [[ "$origin" == "$luban_repo" ]] || fail 'Luban source has a different origin. Existing files are unchanged.'
    changes=$(git -C "$luban_dir" status --porcelain --untracked-files=all) \
        || fail 'Could not check Luban source changes.'
    [[ -z "$changes" ]] || fail 'Luban source has local changes. Commit or move them before rebuilding.'
}

command -v dotnet >/dev/null || sdk_required 'dotnet is not in PATH.'
sdk_version=$(cd "$config_dir" && dotnet --version 2>/dev/null) || sdk_required 'No usable .NET SDK was found in PATH.'
if [[ "$sdk_version" =~ ^([0-9]+)\. ]] && (( 10#${BASH_REMATCH[1]} >= 8 )); then
    :
else
    sdk_required "Active .NET SDK $sdk_version cannot build net8.0."
fi
runtime_list=$(dotnet --list-runtimes 2>/dev/null) || sdk_required 'Could not inspect installed .NET runtimes.'
[[ "$runtime_list" =~ Microsoft[.]NETCore[.]App\ 8[.] ]] || sdk_required '.NET 8 Runtime is missing.'
command -v git >/dev/null || fail 'Git is required to build Luban.'
command -v curl >/dev/null || fail 'curl is required to find the latest Luban release.'
[[ ! -e "$backup_dir" && ! -L "$backup_dir" ]] \
    || fail "$backup_dir already exists. Check the previous Tools backup first."

release_url=$(curl -fsSL -o /dev/null -w '%{url_effective}' \
    'https://github.com/focus-creative-games/luban/releases/latest') \
    || fail 'Could not resolve the latest Luban release. Existing Tools are unchanged.'
if [[ "$release_url" =~ ^https://github[.]com/focus-creative-games/luban/releases/tag/([A-Za-z0-9][A-Za-z0-9._-]*)$ ]]; then
    luban_ref=${BASH_REMATCH[1]}
else
    fail "Unexpected Luban release URL: $release_url"
fi

if [[ -e "$luban_dir" || -L "$luban_dir" ]]; then
    verify_luban_checkout
    printf '[INFO] Selecting Luban %s in "%s"...\n' "$luban_ref" "$luban_dir"
    git -C "$luban_dir" fetch --tags origin "$luban_ref" \
        || fail 'Could not fetch the Luban release. Existing Tools are unchanged.'
    git -C "$luban_dir" switch --detach "$luban_ref" \
        || fail 'Could not select the Luban release. Existing Tools are unchanged.'
else
    printf '[INFO] Cloning Luban %s into "%s"...\n' "$luban_ref" "$luban_dir"
    git clone --depth 1 --branch "$luban_ref" "$luban_repo" "$luban_dir" \
        || fail 'Could not clone Luban. Existing Tools are unchanged.'
fi

if [[ -e "$build_dir" || -L "$build_dir" ]]; then
    rm -rf "$build_dir" || fail "Could not clear $build_dir. Existing Tools are unchanged."
fi

native_config_dir=$config_dir
case "$(uname -s)" in
    MINGW*|MSYS*|CYGWIN*)
        command -v cygpath >/dev/null || fail 'cygpath is required when using Git Bash on Windows.'
        native_config_dir=$(cygpath -m "$config_dir")
        ;;
esac

dotnet build "$native_config_dir/Luban/src/Luban/Luban.csproj" -c Release \
    -o "$native_config_dir/Tools.build" \
    "-p:CustomAfterMicrosoftCommonTargets=$native_config_dir/Extensions/Luban.AlicizaX.Localization/LubanHost.targets" \
    "-p:AlicizaXLocalizationProject=$native_config_dir/Extensions/Luban.AlicizaX.Localization/Luban.AlicizaX.Localization.csproj" \
    "-p:LubanCoreProject=$native_config_dir/Luban/src/Luban.Core/Luban.Core.csproj" \
    || fail 'Luban source build failed. Existing Tools are unchanged.'

for output in Luban.dll Luban.runtimeconfig.json \
    Luban.AlicizaX.Localization.dll Templates/cs-bin/bean.sbn; do
    [[ -f "$build_dir/$output" ]] || fail "Build output is missing $build_dir/$output."
done

cd "$config_dir"
dotnet "$native_config_dir/Tools.build/Luban.dll" -t all --conf luban.conf -f \
    -p alicizax-localization --strict -x outputSaver=alicizax-stage \
    -x alicizax.config=config.ini -x alicizax.validationOnly=true \
    -x l10n.provider=alicizax \
    || fail 'Staged Luban validation failed. Existing Tools are unchanged.'
dotnet "$native_config_dir/Tools.build/Luban.dll" -t client --conf luban.conf \
    -c cs-bin -d bin -p alicizax-localization \
    --customTemplateDir Defines/ClientTemplate --strict \
    -x outputSaver=alicizax-stage -x alicizax.config=config.ini \
    -x l10n.provider=alicizax -x alicizax.check=true \
    || fail 'Staged Luban output differs from generated files. Existing Tools are unchanged.'

had_previous_tools=false
if [[ -e "$tools_dir" ]]; then
    mv "$tools_dir" "$backup_dir" \
        || fail 'Could not back up existing Tools.'
    had_previous_tools=true
fi
if ! mv "$build_dir" "$tools_dir"; then
    if $had_previous_tools; then mv "$backup_dir" "$tools_dir"; fi
    fail 'Could not replace Tools. Previous build was restored if possible.'
fi

if ! bash "$config_dir/gen-all.sh" --check; then
    mv "$tools_dir" "$build_dir" || fail 'Verification failed and new Tools could not be moved aside.'
    if $had_previous_tools; then
        mv "$backup_dir" "$tools_dir" || fail 'Verification failed and previous Tools could not be restored.'
    fi
    fail 'Installed Tools failed verification. Previous build was restored.'
fi

if $had_previous_tools; then
    rm -rf "$backup_dir" || fail "New Tools were installed, but old backup remains at $backup_dir."
fi
verify_luban_checkout
rm -rf -- "$luban_dir" || fail "New Tools were installed, but Luban source remains at $luban_dir."
printf '[INFO] Luban was updated and installed in %s.\n' "$tools_dir"
