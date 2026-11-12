#!/usr/bin/env bash
# Token use of Claude Code subagent runs, read from the local transcripts.
# Prints one row per agent type and model: run count, then average tokens per run.
#
# Usage: Tools/agent-usage.sh [--since YYYY-MM-DD] [--by-day] [--all-projects]
#   --since         count runs that started on or after this date (default: 30 days ago)
#   --by-day        add the start date to each row, to compare before and after a change
#   --all-projects  count runs from all projects (default: Squarely and its worktree slots)
#
# Claude Code deletes transcripts after cleanupPeriodDays (default 30), so older runs are gone.
# Tasks differ from run to run: compare averages over many runs, not single runs.

set -u

SINCE="$(date -d '30 days ago' +%F)"
BY_DAY=0
PROJECT_GLOB="*Squarely*"
while [ $# -gt 0 ]; do
    case "$1" in
        --since) SINCE="$2"; shift ;;
        --by-day) BY_DAY=1 ;;
        --all-projects) PROJECT_GLOB="*" ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
    shift
done

PROJECTS_DIR="$HOME/.claude/projects"

# --- One line per run: date agent model cacheWrite cacheRead output -------------
collect()
{
    for dir in "$PROJECTS_DIR"/$PROJECT_GLOB; do
        [ -d "$dir" ] || continue
        find "$dir" -path "*subagents*" -name "*.meta.json" | while read -r meta; do
            transcript="${meta%.meta.json}.jsonl"
            [ -f "$transcript" ] || continue

            day="$(grep -m1 -o '"timestamp":"[0-9-]*' "$transcript" | cut -d'"' -f4)"
            [ -n "$day" ] || continue
            [[ "$day" < "$SINCE" ]] && continue

            agent="$(grep -o '"agentType":"[^"]*"' "$meta" | head -1 | cut -d'"' -f4)"
            model="$(grep '"type":"assistant"' "$transcript" \
                | grep -o '"model":"claude-[a-z]*' | sort | uniq -c | sort -rn \
                | head -1 | sed 's/.*claude-//')"

            grep '"type":"assistant"' "$transcript" \
                | grep -o '"cache_creation_input_tokens":[0-9]*\|"cache_read_input_tokens":[0-9]*\|"output_tokens":[0-9]*' \
                | awk -F: -v d="$day" -v a="${agent:-?}" -v m="${model:-?}" '
                    { sum[$1] += $2 }
                    END {
                        print d, a, m,
                            sum["\"cache_creation_input_tokens\""] + 0,
                            sum["\"cache_read_input_tokens\""] + 0,
                            sum["\"output_tokens\""] + 0
                    }'
        done
    done
}

# --- Group and print -----------------------------------------------------------
echo "Subagent runs since $SINCE (average tokens per run, in thousands)"
collect | awk -v byDay="$BY_DAY" '
    {
        key = (byDay ? $1 " " : "") $2 " " $3
        runs[key]++
        write[key] += $4
        read[key] += $5
        out[key] += $6
    }
    END {
        if (length(runs) == 0)
        {
            print "No runs found."
            exit
        }
        printf "%-40s %5s %12s %12s %8s\n", "Agent, model", "Runs", "Cache write", "Cache read", "Output"
        for (key in runs)
        {
            n = runs[key]
            printf "%-40s %5d %11dk %11dk %7dk\n", key, n, write[key] / n / 1000, read[key] / n / 1000, out[key] / n / 1000
        }
    }' | { read -r header; echo "$header"; sort; }
