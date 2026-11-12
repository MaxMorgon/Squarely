# Squarely — Code Design

Covers the sim (`Squarely.Simulation`, tests `Squarely.Simulation.Tests`), the Unity presentation layer (`Squarely.Presentation`, see [Presentation](#presentation-unity)) and the app flow (`Squarely.App`, see [App flow](#app-flow-unity)). The meta layer gets its own section later.

## Principles

- **Commands in, events out.** Unity, tests and replays change the sim only through command objects passed to `Sim.Execute(command)`. It returns a `CommandResult` with the events that happened.
- **Read-only from outside.** `Sim` exposes `IReadOnlyBoard` and `IReadOnlyHand`. All mutators are `internal`. The RNG is hidden (only `RandomState` is readable). So outside code cannot break determinism or replays. Tests use `InternalsVisibleTo` (Presentation does the same).
- **Config is data.** No gameplay numbers in code. `SimConfig` and its children are `[Serializable]` classes with public fields (Unity edits them, remote config can load them). A `Sim` deep-copies its config on creation, so config edits apply from the next game and seed plus commands always reproduce a run.
- **Config names rules, a factory builds them.** The config names its dealer, scoring rule and resolution rules by string id. `RuleFactory` maps ids to implementations. A new rule is a new registered id, not a change to the core loop.
- **Rules are stateless.** All state lives in `Sim` (e.g. `PiecesDealt`). So one rule instance can serve a sim and its preview clones.
- **Config vs. seed.** Config is the game mode (authored, shared). The seed is one run of it.

## Folder layout (`Sim/Src/`)

```
Sim.cs          The session: owns state, runs the turn flow
AssemblyInfo.cs InternalsVisibleTo for tests
Core/           GridPos, Grid<T>, Cell, PieceShape, PieceInstance, Board, Hand, read-only interfaces
Patterns/       BoardPattern, PatternCell, PatternMatch, ClearedMatch, PatternMatcher, cell conditions
Commands/       ISimCommand, PlacePieceCommand, EndGameCommand, CommandResult
Replay/         GameReplay (with MoveCount and MatchesEnd), ReplayRecorder, ReplayRunner, ReplayRunResult, ReplayCodec, command codecs
Rules/          IPieceDealer, IScoringRule, ScoreResult, defaults, RuleFactory, RuleIds
Resolution/     IResolutionRule, GameResolution, GameOutcome, NoMovesLeftRule
Config/         ConfigFingerprint, SimConfig, DealingConfig, ClearingConfig, ScoringConfig, PieceDefinition, BoardPatternDefinition, DefaultSimConfig
Events/         ISimEvent + one file per event
Random/         DeterministicRandom
```

## Core types

| Type | Responsibility |
|---|---|
| `GridPos` | `readonly struct` (X, Y). Origin bottom-left. |
| `Grid<T>` | Fixed-size rectangular storage. Shared by `Board` and the board views. |
| `Cell` | `readonly struct` with a string `TypeId`; `null` is empty (so a default cell is empty). String ids let data add cell types. |
| `PieceShape` | Immutable: `Id`, `Offsets` (min X/Y normalized to 0), `Width`, `Height`, `CellTypeId`. Built from text such as `"##/#."`. |
| `PieceInstance` | A dealt piece: sequential (so deterministic) `InstanceId` plus `PieceShape`. Commands use the id, so identical shapes stay distinct. |
| `Board` | Grid of `Cell`. Public reads (`CanPlace`, `CanPlaceAnywhere`, ...). Internal `SetCell`, `Place`, `ClearCells`. |
| `Hand` | Fixed slots of `PieceInstance`. Internal `Add(piece)` fills the first empty slot. `Take(instanceId)` removes a piece. |
| `Sim` | Owns all state, runs the turn flow. Single gameplay entry point: `Execute`, `Preview`. |
| `DeterministicRandom` | Our seeded PRNG (xorshift64*). Not `System.Random`: its output differs between .NET and Unity's Mono. |

## Patterns (clearing)

- A `BoardPattern` is a set of `PatternCell`s (offset + `ICellCondition`), plus `Points` and an optional `Anchor`. An anchored pattern matches at one position. An unanchored one matches anywhere.
- Conditions: `FilledCondition`, `EmptyCondition`, `CellTypeCondition`.
- Full rows and columns are built-in patterns (`ClearingConfig.ClearRows` / `ClearColumns`, `LinePoints`). Custom patterns (pixel art, a smiley, abstract shapes) come from `ClearingConfig.Patterns` as text: `#` filled, `o` must be empty, `.` not in the pattern.
- After a placement, the sim finds all matches and clears all their cells together (each cell once). The view can then "tattoo" a completed image into the background.

## Commands

- `ISimCommand.Execute(Sim)` returns `CommandResult { Success, FailureReason, Events }`.
- Commands are plain data (e.g. `PlacePieceCommand { PieceInstanceId, Origin }`). A command list plus a seed is a full replay.
- A failed command changes no state and has no events.

- `EndGameCommand` (no fields) ends the game as a loss with the `NoMovesLeftRule` resolution (`Lost`, `no_moves_left`) and emits `GameResolvedEvent`. It fails if the game is already resolved.

## Replays

A replay is the seed, the config fingerprint, the successful commands in order, and the score and resolved flag after the last one (`GameReplay`). It can be of a half-finished game.

- **Fingerprint.** `ConfigFingerprint.Compute(SimConfig)` is a 64-bit FNV-1a hash over a fixed byte layout of every config field that changes play. `Sim.ConfigFingerprint` has it for the running sim. New config field: add it to `Compute`. A guard test changes each field in turn and fails if the value does not change. A pinned-value test fails if the layout changes.
- **Recording.** `ReplayRecorder(sim)` wraps `Execute`. It keeps only successful commands. `Build()` gives the replay so far.
- **Code.** `ReplayCodec.Encode/Decode`: `"SQ1."` + base64url of varint bytes. A registry (`CommandCodecRegistry`, `ICommandCodec` with a unique tag) maps command types to tags. New command type: write a codec and register it in `CreateDefault`. `Decode` never throws: `ReplayDecodeResult` has a `ReplayDecodeFailure` (wrong prefix, unknown version, corrupt data, unknown command tag).
- **Running.** `ReplayRunner.Run(replay, config)` runs all commands and reports fingerprint match, first failed index and end-state match. It runs on a fingerprint mismatch. `ReplayRunner.CreateSim(replay, config)` gives a sim at the start for step-by-step playback with `Sim.Execute`.
- **Recording in a game.** `GameController` runs every command (place and end game) through a `ReplayRecorder`. `BuildReplay()` gives the replay of the game so far, also for a half-finished game.
- **Copy replay** (editor and development builds only). `ReplayCopier` (`Presentation/Replay/`, built once in `AppRoot`, in `AppContext.ReplayCopier`) puts a replay code on an `IClipboard`; `SystemClipboard` uses `GUIUtility.systemCopyBuffer`. There is no mobile clipboard yet. The debug overlay has a Copy replay button (and key C), and the game-over screen has one (hidden in other builds). Both show a short "Copied" label. The copy buttons of the test screen history work in all builds. History saving is always on, also in release builds. The game-over texts hold its texts.

## Turn flow — `PlacePieceCommand`

1. Validate: the game is not resolved, the piece instance is in the hand, `Board.CanPlace` succeeds. Otherwise fail.
2. Take the piece from the hand and place it → `PiecePlacedEvent` (piece, slot, origin).
3. Find all matches and clear their cells → `PatternsClearedEvent`: one `ClearedMatch` per match (the match plus the cells it emptied; a cell shared by a row and a column is in both), and all cleared cells, each once.
4. The scoring rule computes points from the matches and updates the combo → `ScoreChangedEvent`, `ComboChangedEvent`.
5. If the hand is empty, the dealer deals a full new hand at once (`IPieceDealer.DealHand`, so it can judge the hand as a whole against the board) → one `PieceDealtEvent` per piece. The default `WeightedRandomDealer` deals by weight, with an optional helpful-piece guarantee (`DealingConfig`; rule in [Design.md](Design.md#dealing)).
6. Check the resolution rules in config order. The first that resolves wins → `GameResolvedEvent` (outcome Won/Lost, reason id, final score).

Unity plays the returned events in order, so the view never diffs state.

**Opening.** A new `Sim` is dealt and ready to play. The constructor deals the opening hand and checks resolution (steps 5 and 6). It keeps the events in `Sim.OpeningEvents`: one `PieceDealtEvent` per piece, then a `GameResolvedEvent` if the game resolves on its first deal. Unity plays them like a command's events.

## Extensibility (next)

- **Mechanics** (blockers, special cells, power-ups) become `IMechanic` hooks at fixed points in the turn flow, created through `RuleFactory`.
- **Board layers** stack grids of cells. The config binds each cell type to a layer (see [Layers](#layers-cell-types-and-theming)). Pattern conditions will check layers.
- **Modes** are config: board, pieces, patterns, rule ids.

## Testing

- `Board`, `Grid`, `Hand`, `PieceShape`, `PatternMatcher`, scoring, `SimConfig` (including its copy), `WeightedRandomDealer` and the RNG have unit tests.
- `Sim` tests use `ScriptedDealer` for exact scenarios (registered through `RuleFactory` like any dealer). `Preview` has its own tests (it equals `Execute` and leaves the state alone).
- Determinism test: the same seed and commands give identical results.

## Presentation (Unity)

Assembly `Squarely.Presentation`. Status: views, drag input and event playback are done. Feedback (hover and preview states driven by `Sim.Preview`) is next. Unity shows sim state and sends commands. It never decides game rules.

### Folder layout (`Unity/Assets/Scripts/Presentation/`)

```
GameController.cs  Plain C#: runs one game, owns the sim, passes commands to it, plays their events, raises GameEndEvent and MoveExecutedEvent
GameView.cs        MonoBehaviour on the Game prefab: builds and sets the views, owns the EventPlayer and registers all its handlers, gizmos
GameSetup.cs       What a game starts with: config and seed
ExecutedMove.cs    A move the game executed: sim before it, events, drop point
AssemblyInfo.cs    InternalsVisibleTo for tests
Config/            SimConfigAsset (SimConfig as an editable asset)
Theme/             CellTheme, CellThemeEntry
Cells/             CellView, CellViewState, CellViewPool, CellStateSpriteTint
Board/             BoardSpace (cells <-> board local space), LayerView, BoardBackground
Camera/            CameraFit (pure fit math), CameraFrame, CameraFitter (keeps the camera framed on board and hand)
Pieces/            PieceView (hand, dragged piece, ghost), HandView
Input/             PieceDragController, IPiecePlacer (what drag input needs from the game), DragSwing (tilt spring), DragFeel (its tunables, `Config/Feel/DragFeel.asset`)
Screens/           MenuScreen, GameOverScreen, TestScreen (with ReplayRow), ReplayScreen, SettingsPanel, ConfirmDialog (report the player's choices as C# events)
Replay/            IClipboard, SystemClipboard, ReplayCopier (replay code to the clipboard)
Hud/               GameHud (settings button, "new record" celebration), ScoreView (score, combo); both on the GameHud prefab
Texts/             TextsConfig and per-screen groups (MenuTexts, GameHudTexts, GameOverTexts, SettingsTexts, ConfirmTexts, TestTexts, ReplayTexts)
Playback/          EventPlayer, PlaybackContext, PlaybackViews, CueSequence, CueRun, CueTarget, GameCues
  Handlers/        ISimEventHandler, SimEventHandler<T>, one handler per event type
  Steps/           CueStep and step types, CellOrder/CellOrdering (stagger order), CueStepPickerAttribute
  Drift/           ShownState, ViewDrift (debug check of views against the sim)
Audio/             SoundCue, SoundBank, SoundPlayer, MusicPlayer, SoundSettings, IHaptics (see "Audio")
Debug/             GameDebugTools (replay, drift check, restart relay, copy replay), PlaybackDebugOverlay (uGUI panel, prefab in `Prefabs/Debug/`); editor and development builds only; not game UI
```

The inspector's step type picker is `Editor/Playback/CueStepPickerDrawer`.

### Scene and prefabs

- **One scene** (`Main.unity`) holds only what every screen shares: the camera, the EventSystem, the entry point. Code creates and destroys screen prefabs.
- **`Prefabs/Game/Game.prefab`** is the whole game: `GameView` with board, hand, input and `GameHud` under it. `GameController` is plain C# created by its owner (`GameState`), so it has no prefab references.
- **`Prefabs/UI/UiCanvas.prefab`** is the base of every UI screen: a Screen Space - Camera canvas scaled for portrait 1080×1920, plane distance 1, on the `UI` sorting layer. That layer is above `Default`, where all world sprites stay. Screens are its prefab variants (`GameHud`, `GameOverScreen`, `MenuScreen`, `TestScreen`). Panels that open over any screen are variants in `Prefabs/UI/`: `SettingsPanel` (sorted above game over) and `ConfirmDialog` (above the panel). Each has a full-screen dimmer that blocks input to everything under it, board and hand included (see "UI blocks world input"). Debug UI (`Prefabs/Debug/PlaybackDebugOverlay`) is a variant sorted above the screens.
- **Canvas camera.** The code that creates a screen sets its canvas's `worldCamera` to the game camera: the states call `AppContext.CreateScreen` for their screens and panels, and `GameView` for the HUD and the debug overlay. Without it, a canvas falls back to overlay rendering.
- **Camera fit.** While a game is up, `CameraFitter` (on the Game prefab) frames the camera on the board and the hand's slots plus a margin, for any aspect. The frame stays below the HUD's `TopBand`, a top-anchored rect in `GameHud`. Only its height counts. The fitter refits in `LateUpdate` when the screen size, the band's share of the screen or the margin changes. The math is the pure `CameraFit.Fit`.

### Principles

- **Owners initialize what they own.** The flow reads in code, not in the scene. An owner creates its children and passes them what they need through an explicit `Initialize(...)` (e.g. `GameView.Initialize(SimConfig, Camera, GameHudTexts, IPiecePlacer, Action<GameResolvedEvent>)`). `Awake`/`Start` only set up the component itself. Never start flow or find other objects there.
- **Views update from events only.** After `Execute`, the sim is already final while the view animates. Views never read sim state during playback.
- **Rule questions go to the sim.** `Sim.Preview` answers "can this be placed?", "what will clear?", "can this hand piece go anywhere?". Unity code never does.
- **Tunable assets live under `Config/<Area>/`** (`Unity/Assets/Config/Gameplay/`, `Texts/`, `Theme/`, ...). Name them for what they are (`ClassicGameMode`, `CellTheme`), not their type.
- **Code says what, assets say how.** Code provides targets and states (cells, centroids, "will clear"). Prefabs, themes and cue sequences decide look and sound.
- **Texts live in one asset.** `TextsConfig` (`Config/Texts/Texts.asset`) holds every player-facing text, in one `[Serializable]` group per screen (each meant to become one Localization string table). `AppRoot` references it once, `AppContext` hands it to the states, and each view gets its group through its `Initialize`. Views pick which text to show and fill format strings (`{0}`). No prefab references the asset.

### Sim.Preview

`Sim.Preview(ISimCommand)` clones the sim state, runs the real command on the clone and returns its `CommandResult`. The real state does not change. It cannot disagree with `Execute`, works for any command and serves AI or rival players. It is cheap enough to call every hover frame. **Implemented.**

- It is a full run: the result holds every event that `Execute` gives.
- It uses the public `Sim.Clone()`: an independent sim in the same state, which multi-move lookahead (AI, tools) can play on freely.
- A clone copies board, hand, RNG state, score, combo, `PiecesDealt` and resolution. It shares rules (stateless) and patterns (immutable).

### Layers, cell types and theming

- **Board layers (planned in Sim, not built).** Any cell type (stone, wood, fire, trap...) can sit in any layer, and all layers work the same way. The config binds each cell type to a layer, and a placed piece goes into its type's layer. Behaviour comes from the cell type's data (blocks placement, cleared by matches), not the layer index. Logic runs through layers in a fixed order, lowest to highest.
- **Layers are one future feature, Sim and view together.** Until then the view has no layer concept: no layer index, list or per-layer sorting.
- **`LayerView`** is a self-contained component with one `Grid<CellView>`. The board uses one instance now (one per layer later). Views are pooled and spawned only where a cell is non-empty.
- **Grid slots do not own views.** On clear, the view leaves the `LayerView` grid and goes to the cue that animates it, so a new piece can land in the same slot at once. When the animation ends, the view returns itself to its pool.
- **One view source.** Board, hand pieces and drag ghost get cell views from the same `CellViewPool`.
- **Themes are modular.** Each area has its own theme asset (`CellTheme` now, e.g. `MenuTheme` later). Views never reference cell view prefabs directly. A `CellTheme` resolves a cell type id to a view prefab, so a theme change is one asset swap. With layers, the theme also sets each layer's sorting order (a Sorting Group on each `LayerView`). Prefabs order only inside their own layer.
- **Cell view states.** Idle, or Ghost (the translucent landing preview while dragging). Code sets the state. The prefab decides how to show it (Animator, tween, shader parameter). Add states when a feature needs them.
- **Board background.** Static empty-slot tiles only. Hover and preview highlights are a separate view.
- **Edit-time previews.** Anything whose size or position comes only from config or serialized values draws a gizmo preview, so the scene is WYSIWYG outside play mode. Such views expose `DrawGizmos(...)` with the config values they need. `GameView` calls them from `OnDrawGizmos` with its game mode, or at edit time with its gizmo preview config (`_gizmoPreviewConfig`). Today: board outline and grid (`BoardBackground`), hand slot pick-up areas (`HandView`), the area the camera keeps on screen (`CameraFitter`).
- **Drag feel is view-only.** `PieceDragController` computes the landing cell from a logical drag target (pointer plus lift), never from the dragged view's transform. The dragged view follows the target in one place (`MoveDraggedView`). Lag, tilt and other feel go there and do not change placement.
- **UI blocks world input.** A press that starts over UI (`EventSystem.current.IsPointerOverGameObject`) never starts a drag. World input that checks this runs in `LateUpdate`, after the EventSystem has processed the frame's press (touch states exist only from then).

### Event playback

`GameController.Place` executes a command and hands its events to the `EventPlayer`. Views change only through handlers. `GameController.StartGame` plays `OpeningEvents` the same way, on the empty views `GameView.Initialize` builds. Only the debug replay sets views straight from a sim state (`GameView.SetState`).

- **`EventPlayer`** (built by `GameView`) plays a command's events in order, one async step at a time (Unity `Awaitable`). It skips events without a handler.
- **One handler per event type** (`PiecePlacedHandler`, `PatternsClearedHandler`, `ScoreChangedHandler`, `ComboChangedHandler`, `PieceDealtHandler`, `GameResolvedHandler`). `GameView.Initialize` registers all of them. A handler updates view bookkeeping at once (spawn, detach, set the hand slot), builds a `CueTarget` (cells with grid positions, a transform, a count, a world position) and plays its `CueSequence` on it through the context. Whatever the views must end up showing happens in the handler or in a cue run (steps or end action), so snapping always reaches it.
- **Shared playback context.** Handlers for one command share a `PlaybackContext`, so later handlers use earlier results (clears stagger from the placed cells; the score target gets the last clear's center). It also carries:
  - `Events`, the command's full event list, so a handler can adapt to what follows (the placement skips its cues when the same command clears);
  - `DropPoint`, presentation-only: where the pointer dropped the piece, in board cell coordinates (`BoardSpace.CellCoordinates`). It passes from `PieceDragController` through `IPiecePlacer.Place` and `EventPlayer.Play`, and is null without a pointer. The placement ripples out from the placed cell nearest to it (`CellOrder.FromPointer`).
- **Per match.** `PatternsClearedHandler` plays its sequence once per match, all at once, each on its own cells and center (`PatternsClearedEvent` gives cleared cells per match). The first match animates a cell shared by two matches.
- **Board waves.** On every placement, `PiecePlacedHandler` also plays a wave on the board's filled cells, outward from the piece (`CellOrder.FromPlacement`): `PlacementWaveCues` if the move clears nothing, `ClearWaveCues` if it clears. The wave skips the cells the command clears (read from `PlaybackContext.Events`), so no later handler takes over a wave cell and `SnapRunsOn` never cuts it short. The placed piece joins the wave only on clearing moves. Otherwise it plays `PiecePlacedCues`, which are skipped when the move clears. The wave's `WaitForEnd` is off, so the clear, score and deal do not wait for it.
- **Overlap.** Each `CueSequence` has `WaitForEnd`. On: the next event waits for it. Off: it plays alongside (score count-up, combo punch). Any flag combination is safe: before a handler detaches or releases cell views, `PlaybackContext.SnapRunsOn` snaps the earlier runs of the same playback that still animate them. So a view never leaves the board or hand while a run animates it. Playback ends when every run has ended. If several consecutive events ever need one handler, add that when a case needs it.
- **`CueSequence`** is a ScriptableObject (`Config/Playback/<Event>Cues`, plus `PlacementWaveCues`, `ClearWaveCues`; `GameView`'s `GameCues` references them): a list of `[SerializeReference] CueStep`s edited in the inspector with a type picker. Steps: `AnimateCells` (scale curve, tint gradient, stagger, `CellOrder`), `AnimateScale`, `CountUp`, `Wait`, `Parallel`, `SpawnParticles`. A new step type is one small class. Steps keep no state and read their settings as they play, so asset edits apply from the next play, also in play mode.
- **Game end.** `GameView` raises `GameResolveEvent` only when the `GameEndGate` opens (see Game over). `GameController` (subscribed in `StartGame`) then raises `GameEndEvent`, so the last move animates before the game-over screen. This is the only path, also for a game that resolves on its first deal (its `GameResolvedEvent` is in `OpeningEvents`).
- **Drift check.** In the editor and development builds, `GameDebugTools` runs it when a playback ends (played out or snapped): `ViewDrift` compares what the views show (`GameView.GetShownState`: board cells, hand, score, combo) with a copy of the sim (`GameController.CopySim`). It logs one error that lists every difference.
- **Debug tools.** `GameDebugTools` (editor and development builds only) is the one debug-only type of a game. `GameState` creates it under `#if` and gives it the controller and the view; `GameController` has no debug code. It keeps the last move (from `GameController.MoveExecutedEvent`, which holds a `Sim.Clone()` taken before the move, and only if someone subscribed) for replay, runs the drift check, and relays the overlay's restart as `RestartRequestEvent` (`GameState` handles it like the game-over Restart). It creates `PlaybackDebugOverlay` through `GameView.CreateDebugOverlay`. The overlay (a uGUI canvas prefab, so it takes mouse and touch through the EventSystem) has: replay of the last move (button or R) with the same drop point; playback speed presets; the last drift check's result; restart; end game (button or E: a debug `EndGameCommand` through the same path as a placement, no-op once resolved); expand and collapse (toggle button or backquote key). It starts collapsed in every game. Speed carries over to the next game and resets to 1x each play session.

### Fast-forward

The player can act during playback. A new command **snaps** everything still pending, then starts its own playback at once. `GameController.Place` snaps before it executes, and `EventPlayer.Play` snaps whatever is pending. Every step loop waits with `PlaybackContext.NextFrameAsync(target)`. It returns without touching views once its run or the playback is snapped (or the views are destroyed).

- Every `CueStep` has `Play` and `Snap`. `Snap` applies the step's end state. It runs on running steps **and on steps that have not started** (a delayed score step snaps the label to its final value).
- Transient things have an empty or trivial `Snap`: particles and sounds already playing finish on their own. A detached view that has not animated goes back to the pool.
- **Open: effect handles.** Effects that one step starts and a later step stops (or that outlive their sequence) need a handle. Ownership, lifetime, what `Snap` does to them and how steps refer to them need a design pass before any step uses them.
- So there is one rule and no per-step modes: board views are correct at once, and effects already playing finish on their own.

### Audio

Folder `Audio/`. Sound is data-driven and plays through Unity mixer groups.

- **`SoundCue`** (ScriptableObject) is one sound: clips (one picked at random per play), volume, pitch variation range, pitch step (semitones per level, with a max level), cooldown, max voices, mixer group and `HapticKind`. `SoundBank` lists all cues and music tracks.
- **`SoundPlayer`** (MonoBehaviour) plays cues on a pool of audio sources. `Play(cue, level)` applies the cue's settings, skips a play inside the cooldown or over the voice limit (`SoundGate`), and sends the haptic to an `IHaptics` (`NoHaptics` by default; no real vibration yet). Pitch math is in `SoundPitch`.
- **`MusicPlayer`** (MonoBehaviour) plays one looping track and crossfades with two sources. `FadeTo` changes the volume of all music while the track keeps playing. Fades use `VolumeFade`.
- **`SoundSettings`** applies the SFX and music toggles to the exposed mixer parameters `SfxVolume` and `MusicVolume` (0 dB on, -80 dB off). It does not save them: App does.
- **`PlaySound`** is a `CueStep`. It plays its cue on `PlaybackViews.Sound` (null: no sound) and does nothing on `Snap`. Its level comes from a `SoundLevelSource`: None, Count (the target's count end), ClearCount (matches cleared by the command) or ComboLevel (combo after the command). `SoundLevelInput` reads these from the target and `PlaybackContext.Events`.
- **Timing for previews.** `CueStep.PlannedSeconds` and `CueStep.CollectSounds` give the time of each step and the sounds it starts. `SoundSchedule.Build` lists the sounds of a sequence from them. A step that takes time overrides `PlannedSeconds`. `AnimateCells` does not count its stagger, as that depends on the cell count.
- **`SoundHooks`** (ScriptableObject, set on the `SoundPlayer`) holds the cues for sounds that no sim event drives, and the music setup. Cues: PiecePickUp, PieceReturn (invalid drop), SnapPreview, ButtonTap, PanelOpen, PanelClose, ToggleOn, ToggleOff, GameLost, GameWon (played when the game-over screen opens, as no cue sequence runs for the resolution), NewBest, NewRecord. A cue left empty plays nothing. Music: the menu track, the game track, the crossfade time, the game-over volume and fade time.
- **Where the players live.** `SoundPlayer` and `MusicPlayer` are scene objects next to `AppRoot`, so they survive screen changes. `AppRoot` holds them and gives them to states through `AppContext` (`Sound`, `Music`, `SoundPrefs`). No static singleton.
- **Who plays the hooks.** `PieceDragController` and `PlaybackViews` get the `SoundPlayer` from `GameView.Initialize`. The drag controller plays PiecePickUp, PieceReturn and SnapPreview. `AppContext` plays the UI hooks: `CreatePanel` and `DestroyPanel` (PanelOpen, PanelClose), `SetSfx` and `SetMusic` (ToggleOn, ToggleOff). `GameState` plays NewBest (with the new-best celebration) and NewRecord (game-over screen).
- **Button sound.** A `ButtonSound` component on a Button plays ButtonTap. `ButtonSound.Bind(root, sound)` connects all of them under a root. `AppContext.CreateScreen` and `GameView.Initialize` call it, so every screen is covered. A screen that makes buttons later (test screen history rows) calls it again.
- **Settings.** `SettingsPanel` has an SFX and a Music toggle. `SoundPreferences` (App) saves them in the `IKeyValueStore` (both on by default), and applies them through `SoundSettings` at start and on change.
- **Music flow.** `MusicDirector` (App) chooses the track: menu and test screen play the menu track; a game and a replay play the game track. A switch crossfades. At game over the music fades down and keeps playing. A new game brings it back to full volume.
- **Haptics.** `AppRoot` gives the `SoundPlayer` a `NoHaptics`. Real vibration is not built.
- **Sound Mixer window** (`Window > Squarely > Sound Mixer`, `Editor/Audio/`) edits the cues of a bank, plays one or many at once, and plays the sounds of a `CueSequence` with `SoundSchedule` timing. It uses hidden temporary audio sources, so it works in Edit mode and Play mode.

## App flow (Unity)

Assembly `Squarely.App`, tests `Squarely.App.Tests`. It decides which screen is up (menu, test, game) and moves between them. The game state also shows the game-over screen. It also owns what the app saves on the device (see [High score and device storage](#high-score-and-device-storage)). It references `Squarely.Presentation` and `Squarely.Simulation`. Presentation never references App and reports through C# events that the states subscribe to.

### Principles

- **The flow reads in code.** Every screen change is a `ChangeState` call in a state class. Nothing in a scene or prefab starts the flow.
- **States own their screens.** A state creates its screen prefabs in `Enter`, initializes them explicitly and destroys them in `Exit`.

### State machine

- `IAppState` has `Enter` and `Exit`. `AppStateMachine.ChangeState(next)` exits the current state, then enters the next.
- `ChangeState` inside `Enter` or `Exit` throws `InvalidOperationException`. A state moves on from a later callback (a button, a screen event). Transitions are synchronous for now. Add async transitions (fades, loading) when a feature needs them.

### Entry point and flow

- **`AppRoot`** is the only scene component that starts anything. It holds the screen prefabs (`AppScreenPrefabs`), camera, default game mode and texts. Its `Start` builds the `SeedSource`, the other services and an `AppContext`, creates the `AppStateMachine` and enters the menu. In editor and development builds it enters the test screen instead if `StartInTest` is on.
- **`AppContext`** holds the refs every state uses: screen prefabs, screen root, camera, default game mode, texts, high score, replay history, replay copier, seed source, test seed, start-in-test option. It has no behaviour.
- **States transition themselves.** Each state takes the context, the state machine and its own arguments (`GameState(context, stateMachine, config, seed, highScore, createBackState)`). It moves on by creating the next state and calling `ChangeState`.
- **`MenuState`** creates `MenuScreen` and starts the default game on `PlayRequestEvent`, against the device high score. **`GameState`** picks the seed (the given one, or a new one from `SeedSource`; it logs it), creates the Game prefab (`GameView`) and a `GameController(view, setup)`, initializes the view (game mode, camera, HUD texts, the controller as its `IPiecePlacer`), then calls `StartGame()`. In editor and development builds it also creates and starts `GameDebugTools`. Each destroys what it created in `Exit`.
- **Settings.** The menu Settings button and the HUD Settings button (`GameHud.SettingsRequestEvent`) open `SettingsPanel`. The state that is up creates and destroys it. Menu: title and Close. Game: Resume, Restart, Quit to menu. Restart and Quit open `ConfirmDialog` ("Are you sure?") once `GameController.HasPlacedPiece` is true. Before that they act at once. They abandon the game and do not record the high score. When the game ends, `GameState` closes the panel and dialog before the game-over screen.
- **Game over.** `GameView` reports the resolution (`GameResolveEvent`) only when a `GameEndGate` opens: the playback that holds the resolution ended (`EventPlayer.PlaybackEndEvent`, which waits for all cue runs, the score count-up too), the "new record" celebration ended (`GameHud.IsCelebrating`, a timer of the length of the serialized `_newRecordClip` that starts with the celebration), and a hold time passed (`GameView._gameEndHold`, 0.5 s; it counts game time, so pause stops it and playback speed scales it). `GameController` turns that into `GameEndEvent` (resolution, final score), once (also for the opening deal). `GameState` then creates `GameOverScreen` over the game. Restart enters a fresh `GameState` with the same game mode and seed setting: a menu game gets a new seed, and a test game keeps its seed. The new Game prefab applies board and hand size edits. Menu enters the back target. **Back target:** `GameState` takes a `Func<IAppState>` for the state the game was started from. `MenuState` passes the menu, `TestState` passes the test screen. Quit (settings) and the game-over Menu button enter it; Restart passes it on to the new `GameState`.
- **Test screen** (editor and development builds only; other builds hide the Test button). `MenuState` enters **`TestState`** on `TestRequestEvent`. `TestState` creates `TestScreen` (`Prefabs/Menu/`): seed field, Randomize, high score field, "start in test screen" toggle, Play, Back. Play enters a `GameState` with the default game mode and exactly the field's seed, so Restart replays that seed. Fields accept digits only. Play is disabled while a field is empty or too big (above `ulong.MaxValue` for the seed, `int.MaxValue` for the high score). Back enters the menu. Quit and game over's Menu return to the test screen.
- **Test high score.** The field is 0 each time the screen opens. The test game plays against `TestHighScore.Create(value)`: a `HighScore` on a `MemoryKeyValueStore`, so it never writes the device high score. Restarts keep the same in-memory high score.
- **`StartInTest`** (`App/StartInTest/`) keeps the toggle in the `IKeyValueStore` (1 or 0, off by default). `TestState` saves it on change. Release builds never read it.
- **`TestSeed`** (`App/TestSeed/`) keeps the test seed in the `IKeyValueStore` as decimal text. `LoadOrCreate` returns the saved seed, or picks and saves a new one. `CreateNew` (Randomize) picks and saves a new one. `TestState` saves the field's seed on Play. An edit without Play is dropped. Any `ulong` can be entered.
- **`SeedSource`** (`App/SeedSource/`) is the one place that picks new seeds, for menu games and for `TestSeed`. Seeds are below 10^9 (easy to read out and type). It wraps a `System.Random` that `AppRoot` creates. The sim never picks a seed: `GameSetup` always has one.

### High score and device storage

- **`IKeyValueStore`** (`App/Storage/`) is everything the app saves on the device: int and string values by key, and `Save`. `PlayerPrefsStore` uses PlayerPrefs. `MemoryKeyValueStore` keeps values in memory only (the test high score and the tests use it). Anything else the app persists goes through it with its own key.
- **`HighScore`** (`App/HighScore/`) is the best score of any finished game, one for all game modes, saved under one key. `Record(score)` returns whether it is new (the first finished game, or above the best) and saves it at once. `AppRoot` builds it on a `PlayerPrefsStore`. `AppContext` hands it to the states.
- **`MenuState`** shows `Best` on the menu (nothing before the first finished game). **`GameState`** records the final score when `GameEndEvent` fires, once per game. It then sets the game-over screen: the "new high score" state, or the high score this game did not beat.
- **New record in game.** `GameState` watches `ScoreView.ScoreChangeEvent` (raised at each step of the score count-up) with a `NewRecordCheck` built from the best score at game start. The first time the shown score goes above it, `GameState` calls `GameHud.CelebrateNewRecord()`, once per game. No best score yet (first game ever) means no celebration. This records nothing. Only the game end records.
- **Celebration hook.** `GameOverScreen` plays its `_newHighScoreCelebration` UnityEvent for a new high score. `GameHud` has the same hook for the in-game record (`_newRecordCelebration`). The art pass hangs animations, particles and sounds on them in the prefab. For now they trigger placeholder Animator clips from `Art/Placeholder/Animations/` (`Celebrate` trigger on the game-over label; `NewRecordPop` on a HUD label that never takes input). Until UI gets its own cue player, UI feedback is animation assets on hooks like this, not code (code-driven animation is for playback cues only).

### Replay history

- **`ReplayHistory`** (`App/ReplayHistory/`) keeps the last 10 games (`ReplayHistoryEntry`: code, UTC save time, score, move count, finished flag), newest first, in one string key of the `IKeyValueStore`. The oldest entry drops past 10. `Entries` reads the list for a screen (the test screen lists it). `Record(replay)` saves a finished game always, and an unfinished game only with at least one placed piece. Entries that do not parse are skipped, so corrupt data never crashes. The clock is a `Func<DateTime>` (wall clock is fine in App, never in Sim).
- **Replays on the test screen.** `TestScreen` has a replays section. `TestState` fills the list from `ReplayHistory.Entries`: one row (`ReplayRow`) per entry with its local date and time, score, move count and finished flag (`ReplayRowFormat`), a Play button and a Copy button. Below the list is a field to paste a replay code with its own Play button. `ReplayPaste.Check` decodes the text and compares the replay's config fingerprint with the default game mode's. `TestState` shows the decode error, or the config warning (Play still works), in the screen's message label. Pasted Play is disabled until the code decodes (`ReplayPasteResult.CanPlay`). All strings are in `TestTexts`.
- **Replay watch** (`ReplayState`, screen `ReplayScreen`; entered from the test screen, Back returns there). It creates the `Game` prefab and a `GameController` with the replay's seed and the default game mode, blocks drag input (`GameView.SetInputBlocked`) and plays the opening deal. It runs each command through `GameController.ExecuteCommand`, the same path as a player move, so the playback is the same. `ReplayStepper` (pure, tested) decides when the next command runs: auto-play waits for the previous playback to end, then `ReplayScreen.StepDelay` (scaled by the playback speed); Step runs the next command at once, also while paused. The watch starts paused. The speed dropdown lists the `ReplayScreen` speed presets and sets `EventPlayer.Speed`. Restart enters a new watch of the same replay. The HUD settings button is hidden (`GameHud.HideSettings`). "Move N / total" counts place commands; at an end-game command it shows the game-ended text. A failed command stops the watch with "Replay diverged at move N". After the last command and its playback, the screen shows the final score and whether it matches the recording (`GameReplay.MatchesEnd`). A banner shows the config warning if the fingerprint differs. The watch records no high score and no history entry, and it has no game-over screen. `ReplayScreen` raises `TickEvent` each frame because states have no update loop.
- **When it saves.** `GameState` records once per game: when the game ends, or in `Exit` if the game was left first (Restart, Quit, debug restart).
