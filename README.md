# Squarely

[![Sim tests](https://github.com/MaxMorgon/Squarely/actions/workflows/sim-tests.yml/badge.svg)](https://github.com/MaxMorgon/Squarely/actions/workflows/sim-tests.yml)

A Unity/C# puzzle game, built with AI agents. The first version took about a week of focused work, part-time over two weeks. I made the design and architecture decisions. Agents wrote most of the code, in over 35 PRs. The result has a deterministic C# sim, replays as paste-able codes and about 400 tests.

> **Gameplay GIF:** coming soon.
> **Play in the browser:** coming soon.

The game: place three pieces at a time on an 8×8 board. Full rows and columns clear. Clears on consecutive moves build a combo. The rules follow the genre (1010!, Block Blast). The code and UI are original. The art is placeholder for now.

## How I work with AI

I use [Claude Code](https://claude.com/claude-code). I set the architecture and the rules, write each brief, and play-test every change. Agents implement, build the Unity assets and review the code.

- **Decisions are mine and on record.** [Docs/Decisions.md](Docs/Decisions.md) lists the key calls, why, and who made them.
- **Rules live in the repo.** [CLAUDE.md](CLAUDE.md) and the [design](Docs/Design.md) and [code design](Docs/CodeDesign.md) docs are the source of truth for every agent.
- **One role per agent** ([.claude/agents/](.claude/agents/)):
  - `implementer` does one brief in a permanent git worktree and opens a PR. Code is the frequent job, so it runs on the cheaper Sonnet.
  - `wirer` builds the prefabs, scenes and config assets on Opus. Unity asset work needs the stronger model.
  - `reviewer` checks each PR against the architecture and its brief. Every 5 PRs, it audits the whole codebase for drift, because a PR review sees only its diff.
- **Skills hold the procedures, agents hold the roles.** Skills in [.claude/skills/](.claude/skills/) are step-by-step procedures that any session loads: running the agents, driving the Unity editor, unattended runs. Agents are separate workers with their own model, tools and context.
- **One file for me.** Agents write only what waits for me into a TODO file: test steps, decisions, questions. I read that list, not an hour of progress logs.
- **Headless checks.** [Tools/verify.sh](Tools/verify.sh) runs the sim tests, then a Unity batch-mode import, compile and test run, in under a minute.
- **Enforced guard rails.** A hook blocks agents from my open Unity editor. Force pushes always ask me.
- **Cost tracking.** [Tools/agent-usage.sh](Tools/agent-usage.sh) reports token use per agent and model.
- **Example:** [audit #38](https://github.com/MaxMorgon/Squarely/issues/38). Each finding became a PR or a recorded decision.

## Engineering highlights

- **Pure C# simulation.** All rules are in a .NET library with no Unity dependency. Unity shows the state and sends commands.
- **Deterministic replays.** A seed plus the command list reproduces a game exactly. A replay is a short text code that you can paste and watch.
- **Preview equals execute.** `Sim.Preview` runs the real command on a clone. Drag input uses it, so the view never re-implements a rule.
- **Data-driven.** Board size, pieces, weights, scoring, clear patterns and rules are config. New rules plug in by id. The core loop stays unchanged.
- **Event-driven presentation.** The sim returns events. Unity plays them as authored cue sequences (animation, sound). A new move snaps the running animations to their end state.
- **Tests guard the contracts:** determinism, preview equals execute, and a config fingerprint that must cover every field.

```
Squarely.App           Screen flow, storage, high score, music
      ▼ depends on
Squarely.Presentation  Views, input, event playback, sound, debug tools
      ▼ depends on, sends commands, plays events
Squarely.Simulation    Rules, dealing, scoring, replays. No Unity.
```

## Tools

> **Replay GIF:** coming soon.

- **Replays.** Open a past game from the replay history and watch the sim play it again, move by move.
- **Sound mixer.** An editor window to balance the sounds. It has one row per sound cue, with volume, pitch, haptics and a play button. It also previews a full cue sequence with its real timing, in Edit mode and in Play mode.

  > **Sound mixer screenshot:** coming soon.

- **Dev tools only where you need them.** Studio builds include the dev tools. A "Dev tools" setting turns them on and off during a test. Store builds compile them out, so a release build has no debug code.

## Run it

Requirements: Unity 6.3 (`6000.3.26f1`) and the .NET 8 SDK.

1. In Unity Hub, add the `Unity/` folder as a project and open it.
2. Open `Assets/Scenes/Main.unity` and press Play.
3. Run the sim tests with `dotnet test Squarely.sln`.

## Next

- Clear preview: highlight the lines a drop would clear.
- Special cells such as stone blockers and bombs, and game modes that use custom clear patterns.
- Daily challenge with a shared seed. Determinism makes server-side score checks from replays possible.

## License

Code: [MIT](LICENSE). Art: placeholder. Audio: CC0 placeholder assets (Kenney, OpenGameArt), listed in [CREDITS.md](Unity/Assets/Audio/CREDITS.md).
