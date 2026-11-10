# Squarely — Code Design

Covers the core sim (namespace `Squarely.Simulation`, tests `Squarely.Simulation.Tests`), the Unity presentation layer (`Squarely.Presentation`, see [Presentation](#presentation-unity)) and the app flow (`Squarely.App`, see [App flow](#app-flow-unity)). The meta layer gets its own section later.

## Principles

- **Commands in, events out.** The outside world (Unity, tests, replays) only changes the sim through command objects passed to `Sim.Execute(command)`. Each returns a `CommandResult` holding the list of events that happened.
- **Read-only from outside.** `Sim` exposes `IReadOnlyBoard` and `IReadOnlyHand`. All mutators are `internal`, and the RNG is hidden (only its `RandomState` is readable), so outside code can't break determinism or replays. Tests reach the internals through `InternalsVisibleTo`.
- **Config is data.** No gameplay numbers in code. `SimConfig` and its children are `[Serializable]` classes with public fields, so Unity can edit them and they can be loaded from remote config.
- **Rules are chosen by config, built by a factory.** The config names its dealer, scoring rule and resolution rules by string id. `RuleFactory` maps ids to implementations. A new rule means registering a new id, not changing the core loop.
- **Rules are stateless.** Dealers, scoring and resolution rules hold no mutable state. All state lives in `Sim` (e.g. `PiecesDealt`), so one rule instance can be shared by a sim and its preview clones, and a sim's state is fully described by its own fields.
- **Config vs. seed.** Config is the game mode (authored, shared). The seed is one run of that mode.

## Folder layout (`Sim/Src/`)

One top-level type per file. The folder listing doubles as an index, for example `Events/` lists every event.

```
Sim.cs          The session: owns state, runs the turn flow
AssemblyInfo.cs InternalsVisibleTo for tests
Core/           GridPos, Grid<T>, Cell, PieceShape, PieceInstance, Board, Hand, read-only interfaces
Patterns/       BoardPattern, PatternCell, PatternMatch, ClearedMatch, PatternMatcher, cell conditions
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
| `Grid<T>` | Fixed-size rectangular storage, one value per `GridPos`. Shared by `Board` and the board views. |
| `Cell` | `readonly struct` with a string `TypeId`, where `null` means empty, so a default cell is empty. String ids let new cell types arrive through data. Board **layers** (e.g. fire) will be separate per-cell grids, independent of the type. |
| `PieceShape` | Immutable definition: `Id`, `Offsets` (normalized so the min X/Y is 0), `Width`, `Height`, `CellTypeId`. Built from a text pattern such as `"##/#."`. |
| `PieceInstance` | One dealt piece: a unique `InstanceId` (sequential, so deterministic) plus its `PieceShape`. Commands refer to pieces by instance id, so two identical shapes in the hand can be told apart. |
| `Board` | Grid of `Cell`. Public reads (`CanPlace`, `CanPlaceAnywhere`, ...). Internal `SetCell`, `Place`, `ClearCells`. |
| `Hand` | Fixed slots of `PieceInstance`. Slots give the view stable positions. Internal `Add(piece)` fills the first empty slot, and `Take(instanceId)` removes a piece. |
| `Sim` | Owns all state and runs the turn flow. The single entry point for gameplay: `Execute` and `Preview`. |
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
3. Find all pattern matches and clear their cells → `PatternsClearedEvent`: one `ClearedMatch` per match (the match plus the cells it emptied, so a cell shared by a row and a column is in both), and all cleared cells, each once.
4. The scoring rule computes points from the matched patterns and updates the combo → `ScoreChangedEvent`, `ComboChangedEvent`.
5. If the hand is now empty, deal a full new hand → one `PieceDealtEvent` per new piece.
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

## Presentation (Unity)

Assembly `Squarely.Presentation`. Status: **views and drag input implemented (step 2). Playback and feedback not yet built.** Unity shows sim state and sends commands. It never decides game rules.

### Folder layout (`Unity/Assets/Scripts/Presentation/`)

```
GameController.cs  Plain C#: runs one game, owns the sim, passes commands to it, raises GameEnded
GameView.cs        MonoBehaviour on the Game prefab: its scene objects and settings, builds and sets the views, gizmos
GameSetup.cs       What a game starts with: config and seed
Config/            SimConfigAsset (SimConfig as an editable asset; edits apply live)
Theme/             CellTheme, CellThemeEntry
Cells/             CellView, CellViewState, CellViewPool, CellStateSpriteTint
Board/             BoardSpace (cells <-> board local space), LayerView, BoardBackground
Pieces/            PieceView (hand, dragged piece, ghost), HandView
Input/             PieceDragController
Screens/           MenuScreen, GameOverScreen (UI screens: report the player's choices as C# events)
Hud/               ScoreView (score and combo, on GameHud)
```

Until playback exists, `GameController` has `GameView` rebuild the board, hand and score from the final sim state after each command.

### Scene and prefabs

- **One scene** (`Main.unity`) holds only what every screen shares: the camera, the EventSystem and the entry point. Screens such as the game and the menu are prefabs, created and destroyed by code.
- **`Prefabs/Game/Game.prefab`** is the whole game: `GameView` with the board, hand, input and `GameHud` under it. The game's logic side, `GameController`, is plain C# created by its owner (`GameState`), so it carries no prefab references.
- **`Prefabs/UI/UiCanvas.prefab`** is the base for every UI screen: an overlay canvas scaled for portrait 1080×1920. Screens are prefab variants of it (`GameHud` is the first).

### Principles

- **Owners initialize what they own.** The flow reads in code, not in the scene. An owner creates its children and passes them what they need through an explicit `Initialize(...)` (e.g. `GameView.Initialize(SimConfig, Camera, GameController)`). `Awake`/`Start` are only for a component setting up itself, never for starting flow or finding other objects.

- **Views update from events only.** After `Execute`, the sim is already in its final state while the view is still animating. Views never read sim state during playback. When playback ends, a debug-only check compares the views with `IReadOnlyBoard` to catch drift.
- **Rule questions go to the sim.** "Can this be placed?", "what will clear?" and "can this hand piece go anywhere?" are answered by `Sim.Preview`, never by Unity code.
- **Code says what, assets say how.** Code provides targets and states (cells, centroids, "will clear"). Prefabs, themes and cue sequences decide how they look and sound.

### Sim.Preview

`Sim.Preview(ISimCommand)` clones the sim state, runs the real command on the clone and returns its `CommandResult` without touching the real state. It can never disagree with `Execute`, works for any command (placement, bombs, future interactions), and also serves AI or rival players. It is cheap enough to call every hover frame. **Implemented.**

- It is a full run: the result holds everything a normal `Execute` would, every event included.
- It is built on the public `Sim.Clone()`, an independent sim in the same state, which multi-move lookahead (AI, tools) can play on freely.
- A clone copies board, hand, RNG state, score, combo, `PiecesDealt` and resolution. It shares rules (stateless) and patterns (immutable).

### Layers, cell types and theming

- **Board layers (planned in Sim, not yet built).** All layers work the same way, and any cell type (stone, wood, fire, trap...) can sit in any layer. The config binds each cell type to a layer, and a placed piece goes into its type's layer. Behaviour comes from the cell type's data (e.g. blocks placement, cleared by matches), not from the layer index. Logic runs through layers in a consistent order (lowest to highest).
- **Layers are one future feature, Sim and view together.** Until then the view has no layer concept at all: no layer index, no layer list, no per-layer sorting.
- **`LayerView`** is a self-contained component that holds one `Grid<CellView>`. For now the board uses a single instance. When layers arrive, it becomes one instance per layer. Views are pooled and spawned only where a cell is non-empty.
- **Views are not owned by grid slots.** A `LayerView` keeps a grid of the current view per slot. On clear, the view is detached (removed from the grid) and handed to the cue that animates it, so a new piece can land in the same slot right away. When its animation ends, the view returns itself to the pool it came from.
- **One view source everywhere.** The board, the hand pieces and the drag ghost all get their cell views from the same `CellViewPool`.
- **Themes are modular.** Each area gets its own theme asset (`CellTheme` now, e.g. `MenuTheme` later). Views never reference cell view prefabs directly. A `CellTheme` asset resolves a cell type id to a view prefab. Changing theme means swapping one asset. When layers arrive, the theme also defines each layer's sorting order (applied through a Sorting Group on each `LayerView`), and prefabs only order things inside their own layer.
- **Cell view states.** Each cell view exposes a state: Idle, or Ghost (the translucent landing preview shown on the board while dragging). Code sets the state, and the prefab decides how to show it (Animator, tween, shader parameter). More states are added when a feature needs them.
- **Board background.** Static empty-slot tiles only. Hover and preview highlights are a separate view.
- **Edit-time previews.** Anything whose size or position comes only from config or serialized values draws a gizmo preview of it, so the scene is WYSIWYG without entering play mode. Such views expose `DrawGizmos(...)` taking the config values they need, and `GameView` calls them from `OnDrawGizmos` with its game mode, or at edit time with its gizmo preview config. Today: board outline and grid (`BoardBackground`), hand slot pick-up areas (`HandView`).
- **Drag feel is view-only.** `PieceDragController` computes the landing cell from a logical drag target (pointer plus lift), never from the dragged view's transform. The dragged view follows the target in one place (`MoveDraggedView`), so lag, tilt or other feel can go there without changing placement.

### Event playback

- **`EventPlayer`** plays a command's events in order, one async step at a time (Unity `Awaitable`).
- **One handler per event type** (`PatternsClearedHandler`, `ScoreChangedHandler`, ...). A handler builds the context (cells, world positions, centroid per match, order along a line) and passes it to a cue sequence.
- **Shared playback context.** Handlers for one command share a context, so later handlers can use earlier results (e.g. the score popup flies from the clear's centroid).
- **Overlap.** By default each handler waits for the previous one to finish. A handler may start without waiting (e.g. the score count-up runs alongside the combo). If several consecutive events ever need to be handled as one, a handler can take them together. That is added when a case needs it.
- **`CueSequence`** ScriptableObject: a list of `[SerializeReference] CueStep`s edited in the inspector, e.g. `AnimateCells(state, stagger, order)`, `SpawnParticles(prefab, at: Centroid)`, `PlaySound`, `Wait`, `Parallel`. A new step type is one small class.
- `PatternsClearedEvent` must give cleared cells **per match**, so each match can have its own centroid and effects.

### Fast-forward

The player can act during playback. A new command **snaps** everything still pending, then starts its own playback right away.

- Every `CueStep` has `Play` and `Snap`. `Snap` applies the step's end state and is called on running steps **and on steps that haven't started yet**. For example, a delayed score step snaps the label to its final value.
- Transient things have an empty or trivial `Snap`: particles and sounds already playing simply finish, and a detached view that hasn't animated yet goes back to the pool.
- **Open: effect handles.** Effects that one step starts and a later step stops (or that outlive their sequence) need a handle. Ownership, lifetime, what `Snap` does to them and how steps refer to them still need a proper design pass before step 3 builds them.
- So there is one rule with no per-step modes: the board views are correct instantly, and effects already playing finish on their own.

### Build order (one PR each)

1. **Sim:** `Sim.Preview(ISimCommand)` and cleared cells per match in `PatternsClearedEvent`, with tests. **Done.**
2. **Views and input:** `CellTheme` and `CellViewPool`, a single `LayerView` with pooled views detached from slots, board background, shared visuals for hand and ghost, and drag-and-drop placement (views rebuild from sim state until playback exists). **Done.**
3. **Playback:** `EventPlayer`, handlers, shared context, `CueSequence` and the first `CueStep`s, fast-forward by snapping.
4. **Feedback:** hover and preview states driven by `Sim.Preview`.

Later: **board layers** as one feature, Sim and view together: cell type definitions with a layer and behaviour flags, one `LayerView` per layer, and per-layer sorting in the theme.

## App flow (Unity)

Assembly `Squarely.App`, tests `Squarely.App.Tests`. Decides which screen is up and moves between them: menu, game, game over. It references `Squarely.Presentation`; Presentation never references App, and reports what happens through C# events that the states subscribe to.

### Principles

- **The flow reads in code.** Every screen change is a `ChangeState` call in a state class. Nothing in a scene or prefab starts the flow on its own.
- **States own their screens.** A state creates its screen prefabs in `Enter`, initializes them explicitly, and destroys them in `Exit`.

### State machine

- `IAppState` has `Enter` and `Exit`. `AppStateMachine.ChangeState(next)` exits the current state, then enters the next.
- Calling `ChangeState` from inside `Enter` or `Exit` throws `InvalidOperationException`: a state moves on from a later callback (a button, a screen event), never while it is being entered or exited. Transitions are synchronous for now; async transitions (fades, loading) come when a feature needs them.

### Entry point and flow

- **`AppRoot`** is the only component in the scene that starts anything. It holds the screen prefabs (`AppScreenPrefabs`), the camera, the default game mode and the seed. Its `Start` builds the default `GameSetup` and an `AppContext`, creates the `AppStateMachine` and enters the menu.
- **`AppContext`** holds the refs every state works with: screen prefabs, screen root, camera, default game setup. It has no behaviour.
- **States transition themselves.** Each state takes the context, the state machine and its own arguments (`GameState(context, stateMachine, setup)`), and moves on by creating the next state and calling `ChangeState`. Reading the states shows the whole flow.
- **`MenuState`** creates `MenuScreen` and starts the default game on `PlayRequested`. **`GameState`** creates the Game prefab (`GameView`), builds a `GameController` from it, the setup and the camera, and calls `StartGame()`. Each destroys what it created in `Exit`.
- **Game over.** `GameController` raises `GameEnded` (resolution, final score) when the game resolves, also if it resolves on its first deal. `GameState` then creates `GameOverScreen` over the game: Restart enters a fresh `GameState` with the same setup (a new Game prefab, so board and hand size edits apply), Menu enters the menu.
- A playtest screen will start games the same way, with a `GameState` built from its own setup.
