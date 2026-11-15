# Squarely

A block-placement puzzle game in Unity and C#. You get three pieces at a time and place them on an 8×8 board. Full rows and columns clear and score. Clear several lines in a row to build a combo. The game ends when no piece fits.

The core rules follow the genre (1010!, Block Blast). The code, art direction, UI and meta features are original. I build it as a learning project that can grow into a live-ops product, and as a test of an AI-agent development workflow.

> **Gameplay GIF:** coming soon.
>
> **Play in the browser:** coming soon.

## Highlights

- **Pure C# simulation.** All game rules are in a plain .NET library with no Unity dependency. Unity only shows the state and sends commands.
- **Deterministic and replayable.** A seed plus a list of commands reproduces any game exactly. Replays encode to a short text code that you can paste and watch.
- **Data-driven.** Board size, pieces, weights, scoring, clear patterns and rules are config, not code. A new rule is a new registered id, not an edit to the core loop.
- **Event-driven presentation.** The sim returns events. Unity plays them as authored cue sequences (animation, sound, particles). The player can act during an animation: playback snaps to its end state.
- **Tested.** About 380 NUnit tests over the sim and the Unity presentation and app logic.

## Architecture

```
Squarely.App           App state machine, screens flow, device storage, high score, music
      │
Squarely.Presentation  Views, drag input, event playback, sound, debug tools
      │
Squarely.Simulation    Board, pieces, dealing, clearing, scoring, resolution, replays (no Unity)
```

Dependencies go down only. The main ideas:

- **Commands in, events out.** `Sim.Execute(command)` returns a result with the events that happened. A failed command changes nothing.
- **Preview equals execute.** `Sim.Preview` runs the real command on a clone. Drag input uses it to check a placement, so the view never re-implements a rule. Future AI players can use it too.
- **Read-only from outside.** The sim exposes read-only board and hand views. Its RNG is a custom seeded PRNG, because `System.Random` gives different output on .NET and Mono.
- **Replay safety.** A fingerprint of the config travels with each replay. Guard tests fail if a new config field is missing from the fingerprint.
- **Two build configs.** Store builds compile all dev tools out. Studio builds keep them (debug overlay, replay copy, drift check that compares the views with the sim) behind a setting.

Details: [Docs/Design.md](Docs/Design.md) (game rules) and [Docs/CodeDesign.md](Docs/CodeDesign.md) (code structure).

## Repository

| Path | Contents |
|---|---|
| `Sim/`, `Sim.Tests/` | The simulation and its tests. Unity uses them as local UPM packages. |
| `Unity/` | The Unity project: presentation, app, editor tools, config assets. |
| `Docs/` | Game design and code design. |
| `Tools/` | Headless verification and workflow scripts. |
| `.claude/` | Agent definitions, skills and hooks for the AI workflow. |

## AI-assisted workflow

I build Squarely with [Claude Code](https://claude.com/claude-code). I make the design and product decisions. Agents write most of the code under rules that I set and review.

- **Rules as files.** [CLAUDE.md](CLAUDE.md) holds the project principles and code style. The design docs are the source of truth that agents read before they work.
- **Specialized agents** in [.claude/agents/](.claude/agents/):
  - `implementer` does one task in a permanent git worktree, verifies it headless and opens a PR.
  - `wirer` builds the prefabs, scenes and config assets for that PR.
  - `reviewer` reviews each PR against the architecture and its brief, and posts inline comments. In audit mode it checks the whole codebase for drift and files an issue.
- **Headless verification.** [Tools/verify.sh](Tools/verify.sh) runs the sim tests, then a Unity batch-mode import, compile and EditMode tests in the agent's worktree.
- **Guard rails.** A hook stops agents from driving the open Unity editor. Agents never run player builds. They commit only from a brief.
- **Visible history.** The pull requests and audit issues of this repo show the loop: brief, implement, review, fix, merge.

## Run it

Requirements: Unity 6.3 (`6000.3.26f1`) and the .NET SDK.

1. Open the `Unity/` folder in Unity Hub.
2. Open `Assets/Scenes/Main.unity` and press Play.
3. Run the sim tests with `dotnet test Squarely.sln`.

## Where I would take it next

- Hover and placement preview feedback driven by `Sim.Preview`.
- Board layers and special cells (blockers, wood, fire) as pluggable mechanics.
- Game modes as config: target score, move limit, custom clear patterns such as pixel-art images.
- Daily challenge with a shared seed, and server-side score validation from replays.
- Meta layer (progression, quests, events) that reacts to sim events, and remote config.

## License

Code: [MIT](LICENSE). Audio: CC0 placeholder assets (Kenney, OpenGameArt), listed in [Unity/Assets/Audio/CREDITS.md](Unity/Assets/Audio/CREDITS.md).
