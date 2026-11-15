# Squarely

[![Sim tests](https://github.com/MaxMorgon/Squarely/actions/workflows/sim-tests.yml/badge.svg)](https://github.com/MaxMorgon/Squarely/actions/workflows/sim-tests.yml)

A block-placement puzzle game in Unity and C#, built with a team of AI agents. I own the architecture, the rules and the reviews. Agents implement, verify and review under those rules.

> **Gameplay GIF:** coming soon.
> **Play in the browser:** coming soon.

The game: you get three pieces at a time and place them on an 8×8 board. Full rows and columns clear and score. Clear on consecutive moves to build a combo. The game ends when no piece fits. The core rules follow the genre (1010!, Block Blast). The code and UI are original. The art is placeholder for now.

## How I build it with AI

I work with [Claude Code](https://claude.com/claude-code). I decide the design and the architecture, write the briefs and review every PR. Agents write most of the code.

- **Rules as files.** [CLAUDE.md](CLAUDE.md) holds the principles and code style. The [design](Docs/Design.md) and [code design](Docs/CodeDesign.md) docs are the source of truth. Agents read them before they work.
- **One role per agent** ([.claude/agents/](.claude/agents/)):
  - `implementer` does one brief in its own git worktree and opens a PR.
  - `wirer` builds the prefabs, scenes and config assets for that PR.
  - `reviewer` checks each PR against the architecture and its brief. In audit mode, it checks the whole codebase for drift.
- **Headless verification.** [Tools/verify.sh](Tools/verify.sh) runs the sim tests, then a Unity batch-mode import, compile and EditMode test run.
- **Guard rails.** A hook stops agents from driving my open Unity editor. Agents never make player builds.
- **Proof in the history.** Example: [PR #37](https://github.com/MaxMorgon/Squarely/pull/37) (brief, review comments, fixes) and [audit #38](https://github.com/MaxMorgon/Squarely/issues/38) (drift findings, each fixed or dropped).

## Engineering highlights

- **Pure C# simulation.** All rules are in a .NET library with no Unity dependency. Unity shows the state and sends commands.
- **Deterministic replays.** A seed plus the command list reproduces a game exactly. A replay is a short text code that you can paste and watch.
- **Preview equals execute.** `Sim.Preview` runs the real command on a clone. Drag input uses it, so the view never re-implements a rule.
- **Data-driven.** Board size, pieces, weights, scoring, clear patterns and rules are config. A new rule is a registered id, not an edit to the core loop.
- **Event-driven presentation.** The sim returns events. Unity plays them as authored cue sequences (animation, sound). A new move snaps the running animations to their end state.
- **Nearly 400 tests.** They guard determinism, preview equals execute, and a config fingerprint that must cover every field.

```
Squarely.App           Screen flow, storage, high score, music
      ▼ depends on
Squarely.Presentation  Views, input, event playback, sound, debug tools
      ▼ depends on, sends commands, plays events
Squarely.Simulation    Rules, dealing, scoring, replays. No Unity.
```

## Run it

Requirements: Unity 6.3 (`6000.3.26f1`) and the .NET 8 SDK.

1. Open the `Unity/` folder in Unity Hub.
2. Open `Assets/Scenes/Main.unity` and press Play.
3. Run the sim tests with `dotnet test Squarely.sln`.

## Next

- Clear preview: highlight the lines a drop would clear.
- Special cells such as stone blockers and bombs, as pluggable mechanics.
- Game modes as config: target score, move limit, pixel-art clear patterns.
- Daily challenge with a shared seed. Determinism makes server-side score checks from replays possible.
- A meta layer (progression, quests, events) that reacts to sim events.

## License

Code: [MIT](LICENSE). Art: placeholder. Audio: CC0 placeholder assets (Kenney, OpenGameArt), listed in [CREDITS.md](Unity/Assets/Audio/CREDITS.md).
