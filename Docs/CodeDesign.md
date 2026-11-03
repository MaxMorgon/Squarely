# Squarely — Code Design (Sim)

Covers the core sim. Namespace: `Squarely.Simulation` (tests: `Squarely.Simulation.Tests`). The Unity and meta layers get their own sections later.

## Principles

- **Commands in, events out.** The outside world (Unity, tests, replays) only changes the sim through command objects passed to `Sim.Execute(command)`. Each returns a `CommandResult` holding the list of events that happened.
- **Read-only from outside.** `Sim` exposes `IReadOnlyBoard` and `IReadOnlyHand`. All mutators are `internal`, and the RNG is hidden (only its `RandomState` is readable), so outside code can't break determinism or replays. Tests reach the internals through `InternalsVisibleTo`.
- **Config is data.** No gameplay numbers in code. `SimConfig` and its children are `[Serializable]` classes with public fields, so Unity can edit them and they can be loaded from remote config.
- **Rules are chosen by config, built by a factory.** The config names its dealer, scoring rule and resolution rules by string id. `RuleFactory` maps ids to implementations. A new rule means registering a new id, not changing the core loop.
- **Config vs. seed.** Config is the game mode (authored, shared). The seed is one run of that mode.

## Folder layout (`Sim/Src/`)

One top-level type per file. The folder listing doubles as an index, for example `Events/` lists every event.

```
Sim.cs          The session: owns state, runs the turn flow
AssemblyInfo.cs InternalsVisibleTo for tests
Core/           GridPos, Cell, PieceShape, PieceInstance, Board, Hand, read-only interfaces
Patterns/       BoardPattern, PatternCell, PatternMatch, PatternMatcher, cell conditions
Commands/       ISimCommand, PlacePieceCommand, CommandResult
Rules/          IPieceDealer, IScoringRule, ScoreResult, defaults, RuleFactory, RuleIds
Resolution/     IResolutionRule, GameResolution, GameOutcome, NoMovesLeftRule
Config/         SimConfig, ClearingConfig, ScoringConfig, PieceDefinition, BoardPatternDefinition, DefaultSimConfig
Events/         ISimEvent + one file per event
Random/         DeterministicRandom
```

## Core types

| Type | Responsibility |
|---|---|
| `GridPos` | `readonly struct` (X, Y). Origin bottom-left. |
| `Cell` | `readonly struct` with a string `TypeId`, where `null` means empty, so a default cell is empty. String ids let new cell types arrive through data. Board **layers** (e.g. fire) will be separate per-cell grids, independent of the type. |
| `PieceShape` | Immutable definition: `Id`, `Offsets` (normalized so the min X/Y is 0), `Width`, `Height`, `CellTypeId`. Built from a text pattern such as `"##/#."`. |
| `PieceInstance` | One dealt piece: a unique `InstanceId` (sequential, so deterministic) plus its `PieceShape`. Commands refer to pieces by instance id, so two identical shapes in the hand can be told apart. |
| `Board` | Grid of `Cell`. Public reads (`CanPlace`, `CanPlaceAnywhere`, ...). Internal `SetCell`, `Place`, `ClearCells`. |
| `Hand` | Fixed slots of `PieceInstance`. Slots give the view stable positions. Internal `Add(piece)` fills the first empty slot, and `Take(instanceId)` removes a piece. |
| `Sim` | Owns all state and runs the turn flow. The single entry point for gameplay. |
| `DeterministicRandom` | Our own seeded PRNG (xorshift64*). `System.Random` is **not** used, because its output differs between .NET and Unity's Mono. |

## Patterns (clearing)

- A `BoardPattern` is a set of `PatternCell`s (offset + `ICellCondition`), plus `Points` as metadata and an optional `Anchor`. Anchored patterns match at one position only; unanchored ones match anywhere.
- The conditions are `FilledCondition`, `EmptyCondition` and `CellTypeCondition`. Layer conditions come later.
- Full rows and columns are just built-in patterns (`ClearingConfig.ClearRows` / `ClearColumns`, `LinePoints`). Custom patterns such as pixel art, a smiley or abstract shapes come from `ClearingConfig.Patterns`, as text where `#` = filled, `o` = must be empty and `.` = not part of the pattern.
- After a placement, all matches are found and all their cells are cleared together (each cell once). The view can then "tattoo" a completed image into the background.

## Commands

- `ISimCommand.Execute(Sim)` returns a `CommandResult { Success, FailureReason, Events }`.
- Commands are plain data (e.g. `PlacePieceCommand { PieceInstanceId, Origin }`), so a list of them plus a seed is a full replay.
- A failed command changes no state and has no events.

## Turn flow — `PlacePieceCommand`

1. Validate: not resolved, the piece instance is in the hand, and `Board.CanPlace` succeeds. Otherwise fail.
2. Take the piece from the hand and place it → `PiecePlacedEvent` (piece, slot, origin).
3. Find all pattern matches and clear their cells → `PatternsClearedEvent` (matches, cleared cells).
4. The scoring rule computes points from the matched patterns and updates the combo → `ScoreChangedEvent`, `ComboChangedEvent`.
5. Refill the hand to `HandSize` → one `PieceDealtEvent` per new piece.
6. Resolution rules are checked in config order, and the first one that resolves wins → `GameResolvedEvent` (outcome Won/Lost, reason id, final score).

Unity plays the returned events in order to animate, so the view never has to diff state.

## Extensibility (next)

- **Mechanics** (blockers, special cells, power-ups) become `IMechanic` hooks at fixed points in the turn flow, created through the `RuleFactory` like the other rules.
- **Board layers** hold per-cell state that is independent of the cell's type. Pattern conditions will be able to check them.
- **Modes** are just a config: board, pieces, patterns and rule ids.

## Testing

- `Board`, `PieceShape`, `PatternMatcher`, scoring and the RNG have unit tests.
- `Sim` tests use `ScriptedDealer`, which is registered through the `RuleFactory` like any other dealer, for exact scenarios.
- A determinism test: the same seed and the same commands must produce identical results.
