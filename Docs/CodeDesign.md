# Squarely — Code Design (Sim)

Covers the core sim. Namespace: `Squarely.Simulation` (tests: `Squarely.Simulation.Tests`). The Unity and meta layers get their own sections later.

## Principles

- **Commands in, events out.** The outside world (Unity, tests, replays) only changes the sim through command objects passed to `Sim.Execute(command)`. Each returns a `CommandResult` holding the list of events that happened.
- **State is plain data.** Board, hand, score and RNG state are simple classes. They are easy to inspect, test, serialize and later snapshot for save, replay or undo.
- **Config is data.** No gameplay numbers in code. Everything tunable comes from `SimConfig`.
- **Strategies for things likely to vary.** Dealing and scoring are behind interfaces, so a new mode or event can swap them without touching the core loop.

## Folder layout (`Sim/Src/`)

```
Sim.cs      The session: owns state, runs the turn flow
Core/       GridPos, Cell, PieceShape, Board, BoardLines, Hand
Commands/   ISimCommand, PlacePieceCommand, CommandResult
Rules/      IPieceDealer, IScoringRule + default implementations
Config/     SimConfig, PieceDefinition, ScoringConfig, DefaultSimConfig
Events/     ISimEvent + event types
Random/     DeterministicRandom
```

## Core types

| Type | Responsibility |
|---|---|
| `GridPos` | `readonly struct` (X, Y). Origin bottom-left. |
| `Cell` | `readonly struct` holding the cell's content `Type` (0 = empty). Later it can grow more state. Board **layers** (e.g. fire) will be separate per-cell grids on the board, independent of the cell's type. |
| `PieceShape` | Immutable. `Id`, `Cells` (offsets, normalized so the min X/Y is 0), `Width`, `Height`. Built from a text pattern such as `"##/#."`. |
| `Board` | Width × Height grid of `Cell`. `CanPlace`, `Place`, `CanPlaceAnywhere`, `FindFullLines`, `ClearLines`. Knows nothing about score or hands. |
| `Hand` | Fixed number of slots (`SimConfig.HandSize`). Each slot holds a `PieceShape` or is empty. |
| `Sim` | Owns board, hand, score, combo, RNG, config, dealer and scoring rule. Runs the turn flow. The single entry point for gameplay. |
| `SimConfig` | Board size, hand size, piece set with weights, scoring values. Plain data, loadable from JSON later. |
| `DeterministicRandom` | Our own seeded PRNG (xorshift64*). `System.Random` is **not** used, because its output differs between .NET and Unity's Mono, which would break determinism. |

## Commands

- `ISimCommand.Execute(Sim)` returns a `CommandResult { Success, FailureReason, Events }`.
- Commands are plain data (e.g. `PlacePieceCommand { Slot, Origin }`), so a list of them plus a seed is a full replay.
- A failed command changes no state.

## Turn flow — `PlacePieceCommand`

1. Validate: not game over, the slot holds a piece, and `Board.CanPlace` succeeds. Otherwise fail.
2. Place on board, empty the slot → `PiecePlacedEvent`.
3. Find and clear full rows and columns together → `LinesClearedEvent`.
4. The scoring rule computes points and the new combo → `ScoreChangedEvent`, `ComboChangedEvent`.
5. **Refill the hand** back to `HandSize` → one `PieceDealtEvent` per new piece.
6. If no piece in the hand fits anywhere → `GameOverEvent`.

Unity plays the returned events in order to animate (place → clear → score popup → new piece), so the view never has to diff state.

## Extensibility (later, but designed for now)

- **New mechanics** (blockers, special cells, power-ups) become `IMechanic` hooks called at fixed points in the turn flow: after placing, after clearing, after refilling. The core loop calls them, and they can add events.
- **Board layers** hold per-cell state that is independent of the cell's type.
- **Modes** are a combination of config, dealer, scoring rule and mechanics.

## Testing

- `Board` gets unit tests for placement bounds, overlap and every clearing case.
- `Sim` gets tests that use a fixed seed or a scripted dealer, for exact, repeatable scenarios.
- A determinism test: the same seed and the same commands must produce identical results.
