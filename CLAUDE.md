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
| `Unity/Assets/Tests/EditMode/` | `Squarely.Presentation.Tests` asmdef: unit tests for presentation logic (layout, pooling, slots). Not for testing how views are wired together. |
| `Unity/Assets/Scripts/App/` | `Squarely.App` asmdef: app state machine and states (menu, game, game over). Design: [Docs/CodeDesign.md](Docs/CodeDesign.md#app-flow-unity). References `Squarely.Presentation`; Presentation never references App. |
| `Unity/Assets/Tests/App/` | `Squarely.App.Tests` asmdef: unit tests for the app flow. |
| `Unity/Assets/Scenes/Main.unity` | The only scene: camera, EventSystem, entry point. Screens are prefabs (`Prefabs/Game/`, `Prefabs/UI/`). Placeholder art in `Art/Placeholder/`, prefabs in `Prefabs/`, the `DefaultCellTheme` in `Themes/`, the `DefaultSimConfig` asset in `Config/`. |
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

## Code style

Standard C# conventions (Microsoft naming: `PascalCase` types/methods/properties, `camelCase` locals/parameters, `_camelCase` private fields, `I` prefix for interfaces), with these preferences:

- **Readability over tightness.** Prefer clear names and an extra line over clever one-liners.
- **Allman braces** — opening brace on its own line.
- **Very short `if` bodies:** two lines, no braces. Never put the body on the same line as the `if`.
- **Regions over comment banners** for grouping members (`#region Public API`), unless a comment is genuinely clearer.
- **Member order:** constants and fields, then properties, then constructors, then methods.
- **`Set…` for data, `Show`/`Hide` for visibility.** A method that takes data and displays it is `Set…` (`SetHand`, `SetShape`), never `Show…`. `Show`/`Hide` only change visibility, and are added when something needs them.

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

- **One top-level type per file** (class, struct, interface, enum), file named after the type. Only exception: a type nested inside another type.
- Namespaces mirror assemblies: `Squarely.Simulation`, `Squarely.Simulation.Tests`. (Not `Squarely.Sim` — it would clash with the `Sim` class.)
- Sources go under `Src/` in each package. Every new file/folder there (and under `Unity/Assets/`) needs a Unity `.meta` file — let Unity generate it; don't hand-write GUIDs. After adding files, run `unity command asset refresh` and commit the generated `.meta` files with them. If the editor isn't connected, ask the user to focus Unity instead.
- Nullable and implicit usings are disabled; write explicit `using` directives.
- New Sim features come with tests in `Sim.Tests`.

## Working with the user

- Work in small steps; show plans/drafts for review before large changes.
- Commit or push only when asked.
