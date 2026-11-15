---
name: queued-run
description: Run a task unattended later. Keeps the PC awake, waits until other Claude sessions and agents are idle, works in resumable phases, then puts the PC to sleep. Use when the user types /queued-run <task>, or asks to resume a queued run.
---

# Queued run

## 1. Plan

1. Split the task into phases. Each phase ends committed and pushed, with the worktree slots clean.
2. Write the plan to `.claude/queue/<task-slug>.md` (gitignored): the task, the phases as checkboxes, and the branch names.
3. Show the plan to the user only if a decision is open. Otherwise continue.

## 2. Keep awake

Run `powershell -File Notes/Tools/keep-awake.ps1 -Hours 8` in the background. It ends by itself after the time limit. If the session stops early (usage limit), the PC still sleeps later. These scripts exist only in the main checkout (private `Notes/` repo).

## 3. Wait

1. Run `ListAgents`. Wait while a peer session in this project shows `busy`, or an agent of this session runs.
2. Check again every 10 minutes. Use `ScheduleWakeup` (or `/loop`).

## 4. Work

- Send a push notification when work starts (`PushNotification`, one line).
- Do the phases in order. After each phase, tick its box in the plan file.
- Follow the batch rules in the `agent-workflow` skill (no progress messages, defaults for non-blocking decisions).
- Put each item that waits for the user in `Notes/TODO.txt`.

## 5. Finish

1. Delete the plan file when all phases are done.
2. Stop the keep-awake process.
3. Send a push notification: the result, and that the PC goes to sleep.
4. Run `powershell -File Notes/Tools/sleep-pc.ps1`.

## Resume

If a plan file exists in `.claude/queue/`, start at the first unchecked phase. First check that the last phase was pushed and the slots are clean.
