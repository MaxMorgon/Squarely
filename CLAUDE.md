# Squarely

A turn-based block-placement puzzle game (learning project, aiming for a live-ops product). The player gets 3 pieces at a time and places them on a grid. Full rows and columns clear and score. Core rules follow the genre. Art, UI and meta features are original.

Read [Docs/Design.md](Docs/Design.md) before you work on game logic. Read [Docs/CodeDesign.md](Docs/CodeDesign.md) for structure, folders and patterns.

## Principles

These four rules decide most cases. The sections below only add details.

1. **The user owns decisions.** Change files only when the user asks. Answer a question with an answer, not an edit. If a request seems wrong, say why and give a better option first. If only a partial fix is possible, ask first. Config assets (`Unity/Assets/Config/`) hold the user's tuning: change an existing value only on request.
2. **One source of truth.** Reuse or generalize an existing type before you add one (e.g. `Grid<T>`, shared by `Board` and `LayerView`). One entry point per flow. One place per rule or fact. Do not merge things that only look alike but change for different reasons.
3. **Sim decides, Unity shows.** All game logic is in Sim. Unity shows Sim state, sends commands and never re-implements rules. Build for extensibility: data-driven config, pluggable mechanics, a sim that emits events.
4. **Brief and plain.** Write all text in ASD-STE100: active voice, short sentences (20 words for instructions, 25 for descriptions), one instruction per sentence. Give only what the reader needs and does not already know.

## Repo layout

| Path | What it is |
|---|---|
| `Sim/`, `Sim.Tests/` | `Squarely.Simulation` (pure C#, no Unity) and its NUnit tests. Unity uses them as local UPM packages. |
| `Unity/Assets/Scripts/Presentation/` | `Squarely.Presentation`: views, input, event playback, sound, debug tools. |
| `Unity/Assets/Scripts/App/` | `Squarely.App`: app state machine, storage, high score, music. App → Presentation → Simulation, never the other way. |
| `Unity/Assets/Tests/` | `EditMode/` (Presentation tests) and `App/` (App tests). |
| `Unity/Assets/Config/` | The user's tuning assets, one subfolder per area. |
| `Unity/Assets/Editor/` | Editor tools and Unity CLI commands (`Cli/`). |
| `Unity/Assets/Scenes/Main.unity` | The only scene. Screens are prefabs. |
| `Notes/` | Private `Squarely-notes` repo (`TODO.txt`, `DevLog.md`). Main checkout only. Not in this repo. |

## Sim rules

- No `UnityEngine` or Unity packages. It builds with plain `dotnet build`.
- C# 9 and netstandard2.1 maximum: no `record`, `init`, file-scoped namespaces, global usings or other C# 10+ features.
- Deterministic: same state and inputs give the same result. No unseeded or shared `System.Random`, wall-clock time, order-dependent `HashSet`/`Dictionary` iteration, or floats in game logic.

## Code style

Standard Microsoft C# naming and conventions, plus:

- Readability over tightness. Allman braces.
- Short `if` bodies: no braces, body on the next line, two lines maximum.
- `#region` for member groups. Member order: constants and fields, properties, constructors, methods.
- `Set…` takes data and displays it. `Show`/`Hide` only change visibility.
- Views set their start state in code (active, alpha, toggles, texts), usually in `Initialize`. Do not rely on how the prefab was saved. Animator states that do not animate a property must not write it (Write Defaults off).
- C# `event` members: `<Noun><Verb>Event`, present tense; handlers `On<Noun><Verb>ed`. Sim event types (`ISimEvent`) are past tense.
- One top-level type per file. Namespaces mirror assemblies (`Squarely.Simulation`, not `Squarely.Sim`).
- Nullable and implicit usings are off. Write explicit `using` directives.
- Doc comments say what (behaviour and contract), not how or who calls it.
- Test-only access: `internal` + `InternalsVisibleTo`, never `public`.

## Tests

- New Sim features come with unit tests in `Sim.Tests`. Also unit-test presentation logic (layout, pooling, slots).
- Do not test view wiring. Write scenario tests only for contracts that fail silently (determinism, preview equals execute). Ask before other kinds.
- After Sim or Sim.Tests changes, run `dotnet test Squarely.sln -v q` once at the end. It must pass.

## Unity

- Every new file and folder under `Src/` or `Unity/Assets/` needs a `.meta`. Let Unity generate it. Never hand-write GUIDs.
- Main checkout: use the Unity CLI (`unity-editor` skill). Worktree slots: `bash Tools/verify.sh <slot>` (no `unity command`).
- The CLI cannot verify how a change looks or plays. Say so and ask the user to check in the editor.
- Gate dev-only code with `#if UNITY_EDITOR || SQUARELY_DEV_TOOLS`, never `DEVELOPMENT_BUILD`. Store builds compile dev tools out. Studio builds compile them in, and the "Dev tools" setting (`DevToolsPreference`) turns them on and off.
- Agents never run a player build.

## Working with the user

- Work in small steps. Show a plan or draft before a large change.
- Commit or push only when asked. An implementer brief counts as asking. `git reset --hard` and force push only on explicit command.
- `Notes/TODO.txt` is the user's only tracker. The user reads no PRs. Never edit its top part. The main session owns the bottom section ("CLAUDE: WHAT I NEED FROM YOU"). It holds only what waits for the user: steps to test and what to see, decisions, questions, actions. No explanations, rationale, status or pointers to other places. Remove items when done.
- Agent work (implementer, wirer, reviewer, batches, review loop, audits): use the `agent-workflow` skill.
