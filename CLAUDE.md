# Squarely

> TODO: One paragraph on the game — genre, core loop, turn-based or real-time, target platform.

## Repo layout

| Path | What it is |
|---|---|
| `Sim/` | Pure C# game logic (`Squarely.Sim`). netstandard2.1, C# 9. No Unity dependency. |
| `Sim.Tests/` | NUnit tests for Sim (`Squarely.Sim.Tests`). Runs via `dotnet test` and in Unity's Test Runner. |
| `Unity/` | Unity 6000.3 (URP 2D) project. Consumes `Sim/Src` and `Sim.Tests/Src` as local UPM packages via `Unity/Packages/manifest.json`. |
| `Squarely.sln` | Solution for Sim + Sim.Tests (Unity has its own generated projects). |

## Architecture rules

- **All game logic lives in Sim.** Unity is presentation + input only: it reads Sim state and sends commands to it.
- **Sim never references `UnityEngine`** or any Unity package. Keep it buildable with plain `dotnet build`.
- **Sim must compile under Unity's constraints:** C# 9 max, netstandard2.1 APIs only. No `record struct`, file-scoped namespaces, global usings, or other C# 10+ features.
- **Sim is deterministic.** Same initial state + same inputs = same result, every time, on every machine. No unseeded or shared `System.Random` (use a seeded RNG owned by the sim state), no `DateTime`/wall-clock time, no iteration over unordered collections (`HashSet`/`Dictionary`) where order affects outcome, no floating-point in game logic unless explicitly agreed.

## Commands

```sh
dotnet build Squarely.sln
dotnet test Squarely.sln -v q
```

After changing Sim or Sim.Tests code, run `dotnet test Squarely.sln -v q` once at the end of the task (not after every edit) and make sure it passes before calling work done. Skip it for doc-only or Unity-only changes. Unity-side changes can't be verified from the CLI — say so and ask the user to check in the editor.

## Code style

Standard C# conventions (Microsoft naming: `PascalCase` types/methods/properties, `camelCase` locals/parameters, `_camelCase` private fields, `I` prefix for interfaces), with these preferences:

- **Readability over tightness.** Prefer clear names and an extra line over clever one-liners.
- **Allman braces** — opening brace on its own line.
- **Very short `if` bodies:** two lines, no braces. Never put the body on the same line as the `if`.
- **Regions over comment banners** for grouping members (`#region Public API`), unless a comment is genuinely clearer.

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

- Namespaces mirror assemblies: `Squarely.Sim`, `Squarely.Sim.Tests`.
- Sources go under `Src/` in each package. Every new file/folder there needs a Unity `.meta` file — let Unity generate it; don't hand-write GUIDs. Mention to the user when new files will need Unity to import them.
- Nullable and implicit usings are disabled; write explicit `using` directives.
- New Sim features come with tests in `Sim.Tests`.

## Working with the user

- Work in small steps; show plans/drafts for review before large changes.
- Commit or push only when asked.
