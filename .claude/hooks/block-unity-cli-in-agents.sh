#!/usr/bin/env bash
# PreToolUse hook (Bash): subagents must not run `unity command ...`.
# The Unity CLI drives the user's open editor on the main checkout; agents use Tools/verify.sh instead.
# Blocks (exit 2) when the call comes from a subagent or from a worktree slot.

INPUT="$(cat)"

if ! printf '%s' "$INPUT" | grep -qiE 'unity(\.exe)?(\\")? +command '; then
    exit 0
fi

if printf '%s' "$INPUT" | grep -qE '"agent_id":"[^"]+"|Squarely-worktrees'; then
    echo "Blocked: subagents must not run 'unity command' (it drives the user's editor). Use Tools/verify.sh <slot> for headless checks." >&2
    exit 2
fi

exit 0
