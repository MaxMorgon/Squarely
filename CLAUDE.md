# Squarely

A turn-based block-placement puzzle game (learning project, aiming for a live-ops product). The player gets 3 pieces at a time and places them on a grid. Full rows and columns clear and score. Core rules follow the genre. Art, UI and meta features are original.

Read [Docs/Design.md](Docs/Design.md) before you work on game logic. Build for extensibility: data-driven config, pluggable mechanics, a sim that emits events.

## Repo layout

| Path | What it is |
|---|---|
| `Sim/` | Pure C# game logic (`Squarely.Simulation`). netstandard2.1, C# 9, no Unity. Design: [Docs/CodeDesign.md](Docs/CodeDesign.md). |
| `Sim.Tests/` | NUnit tests for Sim (`Squarely.Simulation.Tests`). Run by `dotnet test` and Unity's Test Runner. |
| `Unity/` | Unity 6000.3 (URP 2D). Uses `Sim/Src` and `Sim.Tests/Src` as local UPM packages (`Unity/Packages/manifest.json`). |
| `Squarely.sln` | Solution for Sim and Sim.Tests. |
| `Notes/` | Not in this repo: the private `Squarely-notes` repo, cloned here and ignored. Holds `TODO.txt` and `DevLog.md`. Only in the main checkout. |
| `Unity/Assets/Scripts/Presentation/` | `Squarely.Presentation`: views, input, event playback, sound (cues, players), debug tools. References `Squarely.Simulation`, `Unity.InputSystem`, `Unity.TextMeshPro` and `UnityEngine.UI`. [Design](Docs/CodeDesign.md#presentation-unity). |
| `Unity/Assets/Tests/EditMode/` | `Squarely.Presentation.Tests`: unit tests for presentation logic (layout, pooling, slots, playback bookkeeping, cell order, drift check). Not for view wiring. |
| `Unity/Assets/Scripts/App/` | `Squarely.App`: app state machine and states (menu, test, game), device storage (`IKeyValueStore`), high score, seed source, sound preferences, music director. References `Squarely.Presentation` and `Squarely.Simulation`; Presentation never references App. [Design](Docs/CodeDesign.md#app-flow-unity). |
| `Unity/Assets/Tests/App/` | `Squarely.App.Tests`: unit tests for the state machine, storage, high score, new-record check, seed source, test seed and test high score, start-in-test option. |
| `Unity/Assets/Scenes/Main.unity` | The only scene: camera, EventSystem, entry point. Screens are prefabs (`Prefabs/Game/`, `Prefabs/Menu/`, `Prefabs/UI/`). Debug UI: `Prefabs/Debug/`. Placeholder art: `Art/Placeholder/`. |
| `Unity/Assets/Config/` | Assets the user tunes, one subfolder per area: `Gameplay/` (`ClassicGameMode`), `Texts/` (`Texts`), `Theme/` (`CellTheme`), `Feel/` (`DragFeel`), `Audio/` (`SquarelyMixer`, `SoundBank`, `SoundHooks`, `Cues/`; the clips stay in `Assets/Audio/`), `Playback/` (one `CueSequence` per sim event, e.g. `PatternsClearedCues`, plus `PlacementWaveCues`, `ClearWaveCues`). |
| `Unity/Assets/Editor/` | `Squarely.Editor` (Editor only): editor tools and Unity CLI commands (`Cli/`). |

## Architecture rules

- **All game logic lives in Sim.** Unity shows Sim state and sends commands to it.
- **Sim never references `UnityEngine`** or any Unity package. It builds with plain `dotnet build`.
- **Sim compiles under Unity's limits:** C# 9 maximum, netstandard2.1 APIs only. No `record struct`, `record`, `init` accessors (no `IsExternalInit`), file-scoped namespaces, global usings or other C# 10+ features.
- **Sim is deterministic:** same initial state and inputs give the same result on every machine. Do not use:
  - unseeded or shared `System.Random` (use a seeded RNG that the sim state owns);
  - `DateTime` or other wall-clock time;
  - iteration over `HashSet`/`Dictionary` where order changes the outcome;
  - floating-point in game logic, unless agreed.

## Commands

```sh
dotnet build Squarely.sln
dotnet test Squarely.sln -v q
```

The Unity CLI drives the user's open editor. Run it from `Unity/`. The binary is `%LOCALAPPDATA%\Unity\bin\unity.exe`. Pass `--caller plugin --skill unity-cli` to `unity command ...` (`unity status` rejects them).

```sh
unity status --json                      # optional: editor connected? state "ready" (no --caller/--skill flags)
unity command asset refresh              # import files, generate .meta files (custom command)
unity command recompile                  # then poll: unity command recompile_status
unity command console status             # compile errors / console counts
unity command test run --mode EditMode   # long output; read the Summary only
```

Custom commands are `[CliCommand]` static methods in `Unity/Assets/Editor/Cli/` (attributes are in the `Unity.Pipeline.Attributes` assembly). Make a command for each repeated operation. Do not re-send `eval` snippets.

After you change Sim or Sim.Tests code, run `dotnet test Squarely.sln -v q` once at the end of the task, not after each edit. It must pass before the work is done. Skip it for doc-only or Unity-only changes. The CLI cannot verify Unity-side changes: say so, and ask the user to check in the editor.

Builds (needs the platform module; output in `Builds/<Target>-<Config>/`, gitignored). Menu `Squarely/Build/...` or the CLI command `build player <store|studio> <android|webgl>` (`BuildTool`, `Unity/Assets/Editor/`). Agents never run a player build.

**Build configs.** Store has no dev tools: the code is compiled out. Studio is a normal (non-development) build with the scripting define `SQUARELY_DEV_TOOLS`: dev tools are compiled in (debug overlay, replays on game over, Test mode, start-in-test). Gate all dev-only code with `#if UNITY_EDITOR || SQUARELY_DEV_TOOLS`, never `DEVELOPMENT_BUILD`. In such builds the "Dev tools" setting (`DevToolsPreference`, on by default) turns the tools on and off at run time.

Headless checks (no editor open, e.g. a worktree slot; CI runs the Sim part on every PR):

```sh
bash Tools/verify.sh [checkout] [--sim-only]   # dotnet test, then Unity import + compile + EditMode tests; prints errors and totals only
```

Subagent token use per agent and model, from the local transcripts (the user asks for it to compare agent models):

```sh
bash Tools/agent-usage.sh [--since YYYY-MM-DD] [--by-day] [--all-projects]
```

## Code style

Use standard C# conventions (Microsoft naming: `PascalCase` types/methods/properties, `camelCase` locals/parameters, `_camelCase` private fields, `I` prefix for interfaces), with these preferences:

- **Readability over tightness.** Prefer clear names and an extra line to a clever one-liner.
- **Allman braces.**
- **Very short `if` bodies:** two lines, no braces. Never put the body on the `if` line.
- **Regions over comment banners** for member groups (`#region Public API`), unless a comment is clearer.
- **Member order:** constants and fields, properties, constructors, methods.
- **`Set…` for data, `Show`/`Hide` for visibility.** A method that takes data and displays it is `Set…` (`SetHand`, `SetShape`), never `Show…`. `Show`/`Hide` only change visibility. Add them when something needs them.
- **C# events are `<Noun><Verb>Event`, present tense** (`PlayRequestEvent`, `GameEndEvent`, `StateChangeEvent`). Handlers are `On<Noun><Verb>ed` (`OnPlayRequested`, `OnGameEnded`). This applies to C# `event` members only. Sim event types (`ISimEvent`: `ComboChangedEvent`, `GameResolvedEvent`, ...) record what already happened and stay past tense.

## Conventions

- **Reuse before adding.** Before you write a new type, helper or data structure, search for one that does the job, or that you can generalize for both uses (e.g. `Grid<T>`, shared by `Board` and `LayerView`). Do not merge things that only look alike but change for different reasons.
- **One top-level type per file**, named after the type. A nested type is the only exception.
- Namespaces mirror assemblies: `Squarely.Simulation`, `Squarely.Simulation.Tests`. Not `Squarely.Sim`: it clashes with the `Sim` class.
- Sources go under `Src/` in each package. Every new file and folder there (and under `Unity/Assets/`) needs a Unity `.meta` file. Let Unity generate it; never hand-write GUIDs. After you add files, run `unity command asset refresh` and commit the `.meta` files. If the editor is not connected, ask the user to focus Unity. In a worktree slot, run `Tools/verify.sh` instead (its import generates them).
- Nullable and implicit usings are disabled. Write explicit `using` directives.
- **Doc comments say what, not how.** Describe the behaviour and contract. Do not describe internals ("runs on a copy") or one caller's case ("AI players should ignore X"). Mention a limitation only if it matters and the contract does not imply it.
- **Tests:** new Sim features come with tests in `Sim.Tests`. Write unit tests by default (Sim logic, and presentation logic like layout, pooling, slots). Do not test how views are wired together. Write broad scenario tests only for contracts that fail silently (determinism, preview equals execute). Ask before you add others.
- **Test-only access uses `internal` + `InternalsVisibleTo`, never `public`.** Sim, Presentation and App each have an `AssemblyInfo.cs` that opens their internals to their test assembly.

## Parallel agents

Implementation agents ([implementer.md](.claude/agents/implementer.md)) work in permanent worktree slots `A:\WORK\Squarely-worktrees\agent-1` and `agent-2`. The slots have a warm `Unity/Library`. Do not use the Agent tool's `isolation: "worktree"` (cold Library every time).

- Per task: run `git switch -c <branch> origin/<base>` in the slot. When done, leave the slot clean and run `git switch --detach`.
- **Existing branches: detached HEAD only.** Worktrees share local branch refs. If an agent moves a branch that the user has checked out, the user's files go out of sync. So agents work on an existing PR branch with `git switch --detach origin/<branch>` and push with `git push origin HEAD:refs/heads/<branch>`. Only a new branch gets `git switch -c`.
- Agents never run `unity command`: it drives the user's editor on the main checkout, and a hook blocks it. They verify with `Tools/verify.sh <slot>`.
- Agents own their code and assets, open stacked PRs and never merge. Two agents never touch the same scene or prefab.
- **Config assets are the user's.** The files in `Unity/Assets/Config/` hold the user's tuning. Agents can create new config assets. In an existing config asset, they can only set values for fields that they add in the same task. Any other change needs the user's request in the brief, and the PR's **Decisions** lists it.
- **Models and flow.** The `implementer` (Sonnet) writes code and tests. If the task has prefab, scene or config-asset work, the brief says `Wiring: wirer`, and the implementer opens a draft PR with a **Wiring** section. The `wirer` (Opus, [wirer.md](.claude/agents/wirer.md)) does that work in the same slot and marks the PR ready. Then the `reviewer` (Opus) runs. Other tasks use `Wiring: none`. The main session splits a large task into small briefs. Sonnet agents do them one by one on the same branch, and only the last one opens the PR.

## Working with the user

- **Write in ASD-STE100 (Simplified Technical English).** This applies to all text: chat, PR descriptions and comments, issues, docs, doc comments. Use short sentences (20 words maximum for instructions, 25 for descriptions), active voice, simple words, one instruction per sentence. Be brief: give only the points the user needs.
- Work in small steps. Show plans and drafts for review before large changes.
- Commit or push only when asked. An implementer brief counts as asking for that task's branch and PR.
- Use `git reset --hard` and force pushes only on the user's explicit command (settings make them prompt).
- **Who reviews what.** Agents review the code. The user reviews decisions and plays the change. For each implementer PR, the main session runs the `reviewer` agent, then an implementer in review mode on its comments, then hands the PR to the user. The user reads the PR's **Decisions** and **How to test** sections and any `CLAUDE.md` / `CodeDesign.md` changes, and plays the change in the editor. The user opens the code only if something looks wrong.
- **Drift audit.** After a PR merges, the main session counts the PRs merged since the last audit issue (`gh issue list --label audit --state all --limit 1`). At 5 or more, or before a milestone, it runs the `reviewer` agent in audit mode ([reviewer.md](.claude/agents/reviewer.md#audit-mode)) and gives the user the findings. The user picks which become tasks. The main session closes the issue when each finding is a task or dropped.
- **PR review loop.** When the user says "review submitted", the main session runs one implementer in review mode ([implementer.md](.claude/agents/implementer.md#review-mode)) per stack of PRs. The main session decides anything that spans PRs (shared configs, repo-wide rules) and puts it in the brief. On "go", the agents apply the open agreed items, then the main session merges, unless a comment or the message says otherwise. Every PR comment from Claude starts with `**[Claude]**`.
- **`Notes/TODO.txt` is the user's tracker.** The top part is the user's: never edit it. Mention finished items in chat. The main session owns only the section at the bottom ("CLAUDE: WHAT I NEED FROM YOU"). Keep that section current: what to test, open questions, and actions for the user. Remove an item when it is done or answered. Agents never edit `Notes/`. Commit the current `TODO.txt` as-is in the `Notes` repo, and push it, at least once per PR.
