---
name: agent-workflow
description: How the main session runs Squarely agents (implementer, wirer, reviewer). Use before you start an agent or a batch, when the user says "review submitted" or "go", and after a PR merges.
---

# Agent workflow

## Slots and branches

- Implementation agents work in the permanent slots `../Squarely-worktrees/agent-1` and `agent-2` (siblings of the main checkout) (warm `Unity/Library`). Never use the Agent tool's `isolation: "worktree"`.
- New branch: `git switch -c <branch> origin/<base>`. Existing PR branch: detached HEAD only (`git switch --detach origin/<branch>`, push with `HEAD:refs/heads/<branch>`). Worktrees share branch refs.
- At the end the slot is clean and detached.
- Agents never run `unity command` (a hook blocks it). Two agents never touch the same scene or prefab.

## Models and flow

1. Split a large task into small briefs. Sonnet `implementer` agents do them one by one on one branch. Only the last one opens the PR.
2. The brief says `Wiring: wirer` if it has prefab, scene or config-asset work. Then the implementer opens a draft PR with a **Wiring** section, and the Opus `wirer` completes it.
3. Run the `reviewer` agent, then an `implementer` in review mode on its comments.
4. Put each PR's test steps and open decisions in `Notes/TODO.txt` (rules in `CLAUDE.md`).

A brief that changes existing config-asset values must say the user asked for it. The PR's **Decisions** lists it.

## Batches

- No progress messages. Write mid-batch only for a decision that blocks work. Pick a default for the rest and list it at the end.
- End each phase committed and pushed, slots clean. A usage limit can stop the session at any time.
- End with one brief summary, and the test items in `TODO.txt`.

## Review loop

- "review submitted": run one implementer in review mode per stack of PRs. Decide anything that spans PRs (shared configs, repo-wide rules) and put it in the brief.
- "go": the agents apply the agreed open items. Then merge, unless a comment or the message says otherwise.
- Every PR comment from Claude starts with `**[Claude]**`.

## After a merge

- Drift audit: count PRs merged since the last audit issue (`gh issue list --label audit --state all --limit 1`). At 5 or more, or before a milestone, run the `reviewer` in audit mode. Put the findings in `TODO.txt`. Close the issue when each finding is a task or dropped.
- Add a `Notes/DevLog.md` entry: what, why, decided by (user, AI, both). Commit and push `Notes/` (also `TODO.txt`).

## Cost check

When the user asks, run `bash Tools/agent-usage.sh [--since YYYY-MM-DD] [--by-day]` and compare with the baseline in memory.
