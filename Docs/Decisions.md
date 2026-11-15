# Key decisions

The decisions that shaped Squarely, and who made them. **Me**: my call. **AI**: an AI proposal I accepted. **Both**: an AI proposal that I changed.

## Game and architecture

| Decision | Why | By |
|---|---|---|
| The sim is a pure C# library. Unity only shows it. | Logic runs and tests without Unity. Replays, servers and bots can use the same code. | Me |
| The sim is fully deterministic: a custom seeded PRNG, no floats, no wall clock. | Replays, bug reports from a code, shared daily seeds, server checks. `System.Random` differs between .NET and Mono, so we use our own PRNG. | Me (requirement), AI (PRNG) |
| Moves are command objects, results are events. | A seed plus a command list is a full replay. The view plays events and never compares state. | Me |
| The sim is read-only from outside. The RNG is hidden. | The view cannot break determinism behind the sim's back. | Me |
| Clears are patterns, not only full lines. Rows and columns are default patterns. | New puzzle goals (pixel art, shapes) ship as data. | Me |
| Rules (dealer, scoring, win and loss) come from config by id, through a factory. | A game mode is data: board, pieces, patterns, rule ids. | Me |
| `Sim.Preview` runs the real command on a clone. | The view cannot disagree with the rules. The same call serves AI players and lookahead. | AI, extended by me to any command |
| The hand refills only when all 3 pieces are placed. | The player must plan around the whole hand. This is the core tension of the genre. | Me |
| A new move snaps all running animations to their end state. | One rule. The board is always correct at once, and the game feels responsive. | Me (simpler than the AI's two-mode proposal) |
| Animation and sound are authored cue assets, not code. | Designers tune the feel without a code change. | Both |

## AI workflow

| Decision | Why | By |
|---|---|---|
| Project rules live in the repo (`CLAUDE.md`, design docs), not in AI memory. | Every agent and every human reader sees the same rules. | Me |
| Agents work in permanent git worktrees with a warm Unity cache, and verify headless. | Two agents work in parallel. A check takes under a minute, with no full reimport. | Both |
| Code and Unity assets are split: Sonnet `implementer` agents write code, an Opus `wirer` builds prefabs and scenes. | Code is the cheap, frequent job. Unity asset work needs the stronger model. | Me |
| Agents review the code. I review the decisions and play-test each change. | Line-by-line review of agent code added little. Each PR lists its decisions and test steps. | Me |
| A drift audit of the whole codebase every 5 merged PRs. | A PR review sees only its diff. Docs and code drift apart over time. | Both |
| Guard rails are enforced, not written: a hook blocks agents from my open editor. Force pushes always prompt. | A rule in prose can be ignored. A hook cannot. | Me |
