# Squarely — Dev Log

A chronological record of what was built and the decisions behind it. **Decision by** says who drove each call: *you* = the developer, *AI* = proposed by Claude and accepted, *both* = AI proposal shaped by your input.

---

## Session 1 — 2026-11-03

### 1. Project foundation (before AI involvement)
- You set up a solution split into **`Sim`** (pure C#, netstandard2.1), **`Sim.Tests`** (NUnit) and a **Unity 6.3 URP 2D** project. Unity consumes Sim and its tests as local UPM packages with asmdefs (`noEngineReferences: true`).
- **Why it matters:** game logic is testable and runnable without Unity, and the same tests run in both `dotnet test` and Unity's Test Runner. This is the classic "headless simulation + view" architecture used for replays, servers and fast iteration.
- **Decision by:** you.

### 2. Repo and tooling
- The GitHub remote was already connected. Installed the GitHub CLI (`gh`) for PR workflows.
- Added `.gitattributes` for line-ending normalization and binary assets, ignored the Unity-generated IDE files, and silenced the CRLF warnings.
- **Decision by:** both.

### 3. AI working agreement (`CLAUDE.md`)
- Created a project instruction file for AI-assisted development: architecture rules, build and test commands, code style, and workflow.
- Rules you set:
  - **Allman braces**, two-line brace-less short `if`s, never a body on the same line as its `if`.
  - Readability over tightness.
  - `#region` over comment banners.
  - **One top-level type per file** (nested types excepted).
  - Work in small steps, with drafts reviewed before large changes.
- Ways of working you established with the AI:
  - Be token-efficient: no unrequested backups, short answers to side questions.
  - Change code only on explicit instruction, never as a side effect of a question.
  - No half-measure fixes without approval.
  - **Always push back on decisions that seem wrong.**
- **Why it matters:** it treats the AI as a team member with an onboarding doc and working agreements, not as autocomplete.
- **Decision by:** you (style, process); AI (initial structure).

### 4. Game design doc (`Docs/Design.md`)
- Defined the core of a block-placement puzzle: an 8×8 board, pieces that can't rotate, a hand of 3, row and column clears, score with combos, and no-moves-left as the losing condition.
- Framed it as a **live-ops product** from day one: data-driven config, pluggable mechanics, an event stream for the view, analytics and meta, the meta layer separate from the core, and a deterministic seeded sim.
- **Decision by:** you (live-ops vision, genre); AI (doc structure).

### 5. Determinism as an architecture rule
- The sim must be fully deterministic: no unseeded randomness, no wall-clock time, no order-dependent hash iteration, no floats in logic.
- The implementation uses a custom **xorshift64\*** PRNG seeded through SplitMix64, because `System.Random` differs between .NET and Unity's Mono.
- **Why it matters:** it enables replays, reproducible bug reports, shared daily-challenge seeds and later server-side validation.
- **Decision by:** you (requirement); AI (Mono/.NET RNG divergence, PRNG choice).

### 6. Code design (`Docs/CodeDesign.md`): first draft and your revisions
- AI proposal: commands in and events out, strategy interfaces for dealing and scoring, plain-data state.
- Your revisions:
  - **The hand is always kept full**: refill right after each placement instead of after the whole hand is used.
  - **A `Cell` struct instead of an `int`**, so cells can carry state. You also anticipated **board layers** (e.g. a cell "on fire" regardless of its type).
  - **Commands as objects** rather than methods: a seed plus a command list is a full replay.
  - **Renamed `Game` to `Sim`** to avoid ambiguity with the whole app.
- Naming conflict caught: a class `Sim` inside namespace `Squarely.Sim` breaks name resolution from other namespaces. You chose to rename the namespace to **`Squarely.Simulation`**.
- **Decision by:** both, with the key revisions yours.

### 7. Core sim implementation (PR #1, first commit)
- Board, piece shapes defined as text patterns (e.g. `"#./##"`), hand, `Sim` turn flow, command objects, ordered events, weighted random dealer, scoring with a multi-line bonus and combo multiplier, and 37 default pieces.
- 41 NUnit tests, including a **determinism test** in which two games with the same seed and moves must produce identical traces.

### 8. PR-based review workflow
- You asked for a feature branch and a pull request instead of committing to `main`, then reviewed the PR in VS Code with line comments.
- **Why it matters:** this is a professional review workflow applied to AI-written code. The code was reviewed like any teammate's.

### 9. Your PR review: 9 comments, all architectural
Each comment below improved the design:

| Your comment | Outcome | Why it's a strong call |
|---|---|---|
| Place by piece id, not slot | **Piece instances** with unique, deterministic ids (refined after AI pushback: shape ids collide when the hand holds duplicates) | Commands stay valid when slots change, and the sim can reject outdated replay commands |
| Config should be serializable | `[Serializable]` config with public fields | Prepares for editing game modes in the Unity editor and delivering them as remote config, which live ops needs |
| Clear by **patterns**, not just full lines, with points metadata, pixel art and a "tattoo" reward | `BoardPattern` + cell conditions (filled, empty, type, later layers), anchored or anywhere; rows and columns are just default patterns | Turns a hardcoded rule into a content system: new puzzle goals ship as data |
| Cell types: enums or string ids? | String ids | New cell types arrive through data, without a code release |
| Rename `Cells` to `Offsets` | Done | Names say what things are |
| One event per file / quick way to see all events | One type per file; the folder listing is the index | Discoverability |
| Scoring and dealer rules should come from config via a factory | `RuleFactory` + rule ids in config | A game mode becomes pure data: board, pieces, patterns and rule ids |
| Hide the RNG from outside; read-only sim | `IReadOnlyBoard`, `IReadOnlyHand`, hidden RNG, internal mutators + `InternalsVisibleTo` for tests | Protects determinism and replays: the view can't mutate state behind the sim's back |
| Game over should be an external ruleset, called "resolution" | `IResolutionRule` → Won/Lost + reason; `no_moves_left` is the default | Supports objective and level modes (win conditions), not just endless play |

- Result: 53 tests passing, docs updated.

### 10. Live Unity editor automation (Unity CLI)
- Installed Unity's new **Unity CLI** (beta) and the **`com.unity.pipeline`** package, so the AI can drive the *open* editor from the terminal: list commands, read the console, and run the Unity Test Runner.
- Verified it end to end: all 53 sim tests run **inside the live Unity editor** through the CLI and pass, proving the Sim compiles under Unity's compiler as well as .NET.
- You chose the open-editor workflow over headless batch mode, and asked for **custom `[CliCommand]`s for repeated operations** rather than ad-hoc C# `eval` snippets (reviewable, versioned, cheaper). The first one planned is an asset refresh.
- Also installed the .NET 10 SDK for the VS Code C# tooling. Tests verified on it.
- **Decision by:** you (open-editor workflow, commands over eval); AI (setup, verification).

---

## Highlights (raw material for README / CV)

- Designed a **deterministic, headless puzzle simulation** in C#, decoupled from Unity, with full unit-test coverage running in both .NET and Unity.
- Built for **live ops from day one**: data-driven, serializable game modes; a rule factory for pluggable dealers, scoring and win/lose conditions; and a generic **board pattern system** that turns new puzzle goals into data.
- **Command/event architecture**: replayable command objects and an ordered event stream for animation, analytics and meta systems.
- **Defensive API design**: read-only views over sim state and a hidden RNG, so the presentation layer can't break determinism.
- **AI-assisted development with engineering discipline**: a written AI working agreement, explicit style and architecture rules, and PR-based review of all AI-generated code, with architectural feedback driving the design.
