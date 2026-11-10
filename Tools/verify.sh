#!/usr/bin/env bash
# Headless checks for a Squarely checkout: Sim tests, then Unity import + compile + EditMode tests.
# Prints only errors and totals. Exit code 0 = all passed.
#
# Usage: Tools/verify.sh [checkout] [--sim-only]
#   checkout    repo root to check (default: the repo this script is in)
#   --sim-only  skip the Unity step
#
# The Unity step can't run on a project that is open in the editor (it holds Unity/Temp/UnityLockfile).

set -u

ROOT=""
SIM_ONLY=0
for arg in "$@"; do
    case "$arg" in
        --sim-only) SIM_ONLY=1 ;;
        *) ROOT="$arg" ;;
    esac
done
if [ -z "$ROOT" ]; then
    ROOT="$(cd "$(dirname "$0")/.." && pwd)"
fi
ROOT="$(cd "$ROOT" && pwd)" || exit 1

FAILED=0

# --- Sim ---------------------------------------------------------------------
echo "== Sim: dotnet test"
SIM_OUT="$(dotnet test "$ROOT/Squarely.sln" -v q --nologo 2>&1)"
SIM_CODE=$?
echo "$SIM_OUT" | grep -E "error |Failed |Passed!|Failed!|Total tests" | sed 's/^ */   /'
if [ $SIM_CODE -ne 0 ]; then
    echo "   FAILED (exit $SIM_CODE)"
    FAILED=1
fi

if [ $SIM_ONLY -eq 1 ]; then
    exit $FAILED
fi

# --- Unity -------------------------------------------------------------------
PROJECT="$ROOT/Unity"
if [ -e "$PROJECT/Temp/UnityLockfile" ] && ! (exec 3>>"$PROJECT/Temp/UnityLockfile") 2>/dev/null; then
    echo "== Unity: skipped, the project is open in an editor"
    exit 1
fi

VERSION="$(sed -n 's/^m_EditorVersion: *//p' "$PROJECT/ProjectSettings/ProjectVersion.txt" | tr -d '\r')"
UNITY="/c/Program Files/Unity/Hub/Editor/$VERSION/Editor/Unity.exe"
if [ ! -x "$UNITY" ]; then
    echo "== Unity: editor $VERSION not found at $UNITY"
    exit 1
fi

LOGS="$PROJECT/Logs"
mkdir -p "$LOGS"
LOG="$LOGS/verify.log"
RESULTS="$LOGS/verify-editmode.xml"
rm -f "$RESULTS"

echo "== Unity $VERSION: import, compile, EditMode tests"
"$UNITY" -batchmode -nographics -projectPath "$PROJECT" -runTests -testPlatform EditMode \
    -testResults "$RESULTS" -logFile "$LOG" > /dev/null 2>&1
UNITY_CODE=$?

grep -E "error CS[0-9]+" "$LOG" | sort -u | sed 's/^/   /'
if [ -f "$RESULTS" ]; then
    grep -o '<test-run [^>]*>' "$RESULTS" \
        | grep -oE '(total|passed|failed|skipped)="[0-9]+"' | tr '\n' ' ' | sed 's/^/   /'
    echo
    grep -oE '<test-case [^>]*result="Failed"[^>]*>' "$RESULTS" \
        | grep -oE 'fullname="[^"]*"' | sed 's/^/   FAILED /'
else
    echo "   no test results (compile errors or Unity failed; see $LOG)"
fi
if [ $UNITY_CODE -ne 0 ]; then
    echo "   FAILED (exit $UNITY_CODE)"
    FAILED=1
fi

exit $FAILED
