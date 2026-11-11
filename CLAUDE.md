# Squarely

A block-placement puzzle game (learning project, aiming for a live-ops product). The player is dealt 3 pieces at a time to place on a grid; full rows/columns clear and score. Turn-based. Core rules follow the genre; art, UI and meta features are original.

Design and rules: [Docs/Design.md](Docs/Design.md) — read it before working on game logic. Build for extensibility: data-driven config, pluggable mechanics, sim emits events for Unity/meta to consume.

## Repo layout

| Path | What it is |
|---|---|
| `Sim/` | Pure C# game logic (namespace/assembly `Squarely.Simulation`). netstandard2.1, C# 9. No Unity dependency. Code design: [Docs/CodeDesign.md](Docs/CodeDesign.md). |
| `Sim.Tests/` | NUnit tests for Sim (`Squarely.Simulation.Tests`). Runs via `dotnet test` and in Unity's Test Runner. |
| `Unity/` | Unity 6000.3 (URP 2D) project. Consumes `Sim/Src` and `Sim.Tests/Src` as local UPM packages via `Unity/Packages/manifest.json`. |
| `Squarely.sln` | Solution for Sim + Sim.Tests (Unity has its own generated projects). |
| `Unity/Assets/Scripts/Presentation/` | `Squarely.Presentation` asmdef: views, input, event playback. Design: [Docs/CodeDesign.md](Docs/CodeDesign.md#presentation-unity). References `Squarely.Simulation` and `Unity.InputSystem`. |
| `Unity/Assets/Tests/EditMode/` | `Squarely.Presentation.Tests` asmdef: unit tests for presentation logic (layout, pooling, slots, playback bookkeeping, drift check). Not for testing how views are wired together. |
| `Unity/Assets/Scripts/App/` | `Squarely.App` asmdef: app state machine and states (menu, game, game over). Design: [Docs/CodeDesign.md](Docs/CodeDesign.md#app-flow-unity). References `Squarely.Presentation`; Presentation never references App. |
| `Unity/Assets/Tests/App/` | `Squarely.App.Tests` asmdef: unit tests for the app flow. |
| `Unity/Assets/Scenes/Main.unity` | The only scene: camera, EventSystem, entry point. Screens are prefabs (`Prefabs/Game/`, `Prefabs/Menu/`, `Prefabs/UI/`), debug UI in `Prefabs/Debug/`. Placeholder art in `Art/Placeholder/`, prefabs in `Prefabs/`. |
| `Unity/Assets/Config/` | Every asset the user tunes, one subfolder per area: `Gameplay/` (`ClassicGameMode`), `Texts/` (`Texts`), `Theme/` (`CellTheme`), `Playback/` (one `CueSequence` per sim event, e.g. `PatternsClearedCues`). |
| `Unity/Assets/Editor/` | `Squarely.Editor` asmdef (Editor-only): editor tools and custom Unity CLI commands (`Cli/`). |

## Architecture rules

- **All game logic lives in Sim.** Unity is presentation + input only: it reads Sim state and sends commands to it.
- **Sim never references `UnityEngine`** or any Unity package. Keep it buildable with plain `dotnet build`.
- **Sim must compile under Unity's constraints:** C# 9 max, netstandard2.1 APIs only. No `record struct`, file-scoped namespaces, global usings, or other C# 10+ features. Also no `record` or `init` accessors (netstandard2.1 lacks `IsExternalInit`).
- **Sim is deterministic.** Same initial state + same inputs = same result, every time, on every machine. No unseeded or shared `System.Random` (use a seeded RNG owned by the sim state), no `DateTime`/wall-clock time, no iteration over unordered collections (`HashSet`/`Dictionary`) where order affects outcome, no floating-point in game logic unless explicitly agreed.

## Commands

```sh
dotnet build Squarely.sln
dotnet test Squarely.sln -v q
```

Unity CLI (drives the user's open editor; run from `Unity/`, binary at `%LOCALAPPDATA%\Unity\bin\unity.exe`, pass `--caller plugin --skill unity-cli` to `unity command ...` calls; `unity status` rejects them):

```sh
unity status --json                      # optional: editor connected? state "ready" (no --caller/--skill flags)
unity command asset refresh              # import new/changed files, generate .meta files (custom command)
unity command recompile                  # then poll: unity command recompile_status
unity command console status             # compile errors / console counts
unity command test run --mode EditMode   # run tests in the editor (long output; read the Summary only)
```

Custom commands are `[CliCommand]` static methods in `Unity/Assets/Editor/Cli/` (attributes come from the `Unity.Pipeline.Attributes` assembly). Turn any operation we repeat into one, rather than re-sending `eval` snippets.

After changing Sim or Sim.Tests code, run `dotnet test Squarely.sln -v q` once at the end of the task (not after every edit) and make sure it passes before calling work done. Skip it for doc-only or Unity-only changes. Unity-side changes can't be verified from the CLI — say so and ask the user to check in the editor.

Headless checks (for a checkout with no editor open, e.g. a worktree slot; CI runs the Sim part on every PR):

```sh
bash Tools/verify.sh [checkout] [--sim-only]   # dotnet test, then Unity import + compile + EditMode tests; prints errors and totals only
```

## Code style

Standard C# conventions (Microsoft naming: `PascalCase` types/methods/properties, `camelCase` locals/parameters, `_camelCase` private fields, `I` prefix for interfaces), with these preferences:

- **Readability over tightness.** Prefer clear names and an extra line over clever one-liners.
- **Allman braces** — opening brace on its own line.
- **Very short `if` bodies:** two lines, no braces. Never put the body on the same line as the `if`.
- **Regions over comment banners** for grouping members (`#region Public API`), unless a comment is genuinely clearer.
- **Member order:** constants and fields, then properties, then constructors, then methods.
- **`Set…` for data, `Show`/`Hide` for visibility.** A method that takes data and displays it is `Set…` (`SetHand`, `SetShape`), never `Show…`. `Show`/`Hide` only change visibility, and are added when something needs them.
- **C# events are `<Noun><Verb>Event`, present tense** (`PlayRequestEvent`, `GameEndEvent`, `StateChangeEvent`), and their handlers `On<Noun><Verb>ed` (`OnPlayRequested`, `OnGameEnded`). This is for C# `event` members only. Sim event types (`ISimEvent`: `ComboChangedEvent`, `GameResolvedEvent`, ...) record what already happened in a command and stay past tense.

```csharp
public int Clamp(int value)
{
    if (value < 0)
        return 0;

    if (value > Max)
    {
        Log("Clamped");
        return Max;
    }

    return value;
}
```

Not: `if (value < 0) return 0;`

## Conventions

- **Reuse before adding.** Before writing a new type, helper or data structure, search for one that already does the job, or that can be cleanly generalized to cover both uses (e.g. `Grid<T>`, shared by `Board` and `LayerView`). Don't merge things that only look alike but change for different reasons.
- **One top-level type per file** (class, struct, interface, enum), file named after the type. Only exception: a type nested inside another type.
- Namespaces mirror assemblies: `Squarely.Simulation`, `Squarely.Simulation.Tests`. (Not `Squarely.Sim` — it would clash with the `Sim` class.)
- Sources go under `Src/` in each package. Every new file/folder there (and under `Unity/Assets/`) needs a Unity `.meta` file — let Unity generate it; don't hand-write GUIDs. After adding files, run `unity command asset refresh` and commit the generated `.meta` files with them. If the editor isn't connected, ask the user to focus Unity instead. In a worktree slot, run `Tools/verify.sh` instead (its import generates them).
- Nullable and implicit usings are disabled; write explicit `using` directives.
- **Doc comments say what, not how.** Describe the behaviour and contract, not internals ("runs on a copy") or one caller's case ("AI players should ignore X"). Mention a limitation only if it matters and the contract doesn't imply it.
- **Tests:** new Sim features come with tests in `Sim.Tests`. Unit tests by default (Sim logic, and presentation logic like layout, pooling, slots). No tests that pin how views are wired together. Broad scenario tests only for contracts that fail silently (determinism, preview equals execute); ask before adding others.

## Parallel agents

Implementation agents (`.claude/agents/implementer.md`) work in permanent worktree slots `A:\WORK\Squarely-worktrees\agent-1` and `agent-2`, which have a warm `Unity/Library`. Not the Agent tool's `isolation: "worktree"` (cold Library every time).

- Per task: `git switch -c <branch> origin/<base>` in the slot; when done, leave it clean and `git switch --detach`.
- Agents never run `unity command` (it drives the user's editor on the main checkout; a hook blocks it). They verify with `Tools/verify.sh <slot>`.
- Agents own their code and assets, open stacked PRs, and never merge. Two agents don't touch the same scene or prefab.

## Working with the user

- Work in small steps; show plans/drafts for review before large changes.
- Commit or push only when asked. An implementer brief counts as asking for that task's branch and PR.
- `git reset --hard` and force pushes only on the user's explicit command (settings make them prompt).
- **Who reviews what.** Agents review the code; the user reviews decisions and plays the change. When an implementer opens a PR, the main session runs the `reviewer` agent on it, then an implementer in review mode on its comments, before handing the PR to the user. The user reads the PR's **Decisions** and **How to test** sections and any `CLAUDE.md` / `CodeDesign.md` changes, plays it in the editor, and opens the code only when something there looks off.
- **Drift audit.** After merging a PR, the main session checks how many PRs have been merged since the last audit issue (`gh issue list --label audit --state all --limit 1`). At 5 or more, or before a milestone, it runs the `reviewer` agent in audit mode ([reviewer.md](.claude/agents/reviewer.md#audit-mode)) and gives the user the findings. The user picks which ones become tasks, and the main session closes the issue once each finding is either a task or dropped.
- **PR review loop.** When the user says "review submitted": the main session runs implementers in review mode ([implementer.md](.claude/agents/implementer.md#review-mode)), one per stack of PRs. Each one checks every comment on its merits, fixes and resolves the ones it agrees with that are simple, and replies only where it pushes back or the result isn't obvious, leaving those threads open. The main session decides anything that spans PRs (shared configs, repo-wide rules) and passes it in the brief. On "go": the open agreed items are applied, then the main session merges, unless a comment or the message says otherwise. Every PR comment Claude writes starts with `**[Claude]**`.
- **`TODO.txt` is the user's tracker.** Don't read or edit it unless asked; mention finished items in chat instead. Commit the user's current version as-is, in its own commit, at least once per PR.
