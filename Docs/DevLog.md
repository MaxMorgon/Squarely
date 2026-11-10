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

### 11. PR housekeeping
- An accidental merge order (the stacked PR merged into its base branch first) was untangled. The feature branch was reset, the work re-opened as a **draft** stacked PR so it couldn't be merged early, then retargeted to `main` once its base had merged.
- You granted scoped permissions (`git reset --hard`, `git push --force-with-lease`) in local, uncommitted settings instead of blanket access.

### 12. First custom CLI command + presentation assembly
- Added **`unity command asset refresh`**, a project-specific `[CliCommand]` that imports new files and generates `.meta` files in the open editor. This removes the manual "switch to Unity" step from the AI workflow. Tested end to end with a temporary file.
- Found and worked around a documentation error: the CLI attributes live in `Unity.Pipeline.Attributes`, not `Unity.Pipeline`.
- Prepared the **`Squarely.Presentation`** asmdef (references Sim + Input System) and the Editor-only **`Squarely.Editor`** asmdef, ready for the view layer.
- **Decision by:** both.

---

## Session 2 — 2026-11-04

### 13. Presentation architecture design (`Docs/CodeDesign.md` → Presentation)
- Designed the Unity view layer before writing any code. The result is recorded as an agreed design with a four-PR build order.
- Key decisions:

| Decision | Decision by | Why it matters |
|---|---|---|
| A self-contained `LayerView` (one grid of cell visuals) that later becomes one per board layer | you (layer per view); AI (keep the component, drop the layer plumbing) | Planned mechanics (fire, traps) slot in without restructuring views, with no speculative code now |
| All layers are the same: any cell type can sit in any layer, the config binds type to layer, behaviour comes from type data | you (AI: behaviour flags on the type, not the layer index) | No special-cased "main layer"; new cell types are pure data |
| Layers built later as one feature, Sim and view together; no layer concept in the view until then | you | Avoids half-built features and guessing at a layer design that doesn't exist yet |
| Visuals detached from grid slots and pooled, the same visuals for board, hand and ghost | AI | Clear animations can keep playing while the next piece lands in the same slot |
| `Theme` asset resolves visuals, and later per-layer sorting order | you (theming); AI (sorting in the theme rather than per prefab, so one prefab can't break layer order) | Reskins and live-ops themes are an asset swap |
| `Sim.Preview(ISimCommand)`: run the real command on a cloned state | AI; you extended it to any command and AI/rival players | Hover feedback can never disagree with the real rules; it doubles as an AI lookahead tool |
| Event playback through handlers and editor-authored `CueSequence` assets | AI (structure); you (designer-editable animations, own system over a paid asset) | Designers tune the feel of the game without code changes |
| Fast-forward: a new command always snaps, including steps not yet started; transient effects just finish | you (simpler than AI's two-mode proposal; snapping, not skipping, unstarted steps) | One rule, so the board is correct instantly and the game feels responsive |
| One PR per build step, one chat per PR, the design written into the repo first | you (PR/chat split); AI (persist the design for fresh contexts) | Keeps AI context small and reviews focused |

### 14. `Sim.Preview` and per-match clears (build step 1)
- **`Sim.Preview(command)`** runs the real command on a private clone of the sim (board, hand, RNG state, score, combo, deal count, resolution) and returns its result. The real sim is untouched. It is a full run, so its result holds everything `Execute` would.
- **`Sim.Clone()`** is public, so multi-move lookahead (AI, tools) can play on an independent copy. **Decision by:** you (public clone for consistency and flexibility; doc comments say what an API does, not how).
- **Rules are now stateless by contract.** The AI found that a stateful rule (the test `ScriptedDealer`, which dequeued pieces) would be used up by previews. The options were stateless rules, cloning rules, or recreating them from the factory. The dealer now receives the `Sim` (`Deal(Sim, DeterministicRandom)`, the same as resolution rules), and `Sim.PiecesDealt` lets the scripted dealer pick by index. **Decision by:** you (AI's recommendation). **Why it matters:** all state lives in `Sim`, which snapshots and save/load will also need.
- **`PatternsClearedEvent`** now holds one `ClearedMatch` per match, with the cells it emptied, so each match can get its own centroid and effects.
- Tests: preview equals execute for every move of a full game, previewing every hover position each turn leaves the game trace identical, and previews never consume dealt pieces or resolve the real sim.

---

## Session 3 — 2026-11-05

### 15. Hand refills when empty, step 2 scope
- **The hand refills only when it is empty.** After the last of the 3 pieces is placed, a full new hand is dealt, instead of each slot refilling right away. **Decision by:** you. **Why it matters:** the player has to plan around all three pieces, which is the genre's core tension.
- Step 2 scope refined before building:
  - **Drag-and-drop placement** moves into step 2, so the views and ghost get exercised for real. Until playback exists, the views rebuild from sim state after each command. **Decision by:** you (AI proposed both options).
  - **Ghost** = the translucent landing preview snapped to the board while dragging, separate from the dragged piece. **Visual states** are only Idle and Ghost until a feature needs more. **Decision by:** you.
  - **The board background is empty-slot tiles only.** Highlights get their own view. **Decision by:** you.
  - **A single cell colour for now.** **Decision by:** you.
  - **Presentation tests are unit tests** (layout math, pooling, slots), not integration tests of how they're wired together, since that wiring is expected to change. **Decision by:** you.
- **`RefillHand` became `DealNewHand`.** The emptiness check moved to the caller (`if (_hand.IsEmpty)`), so the method does what its name says, and `IReadOnlyHand.IsEmpty` is new. **Decision by:** you (AI's version silently did nothing on a non-empty hand).

### 16. Views and drag-and-drop (build step 2)
- **`Theme`** maps cell type ids to `CellVisual` prefabs (with a fallback) and holds the empty-slot tile. **`CellVisualCatalog`** is the one source of visuals for the board, the hand, the dragged piece and the ghost, with one `ObjectPool` per prefab. A visual returns itself with `Release()`.
- **`LayerView`** holds board visuals in a `SlotGrid`. `Detach` hands a visual over without releasing it, ready for clear animations in step 3. **`BoardBackground`** only draws tiles.
- **`PieceDragController`**: picking up a hand piece lifts it above the pointer, and the ghost snaps to the board wherever `Sim.Preview` says the move is legal. Releasing places the piece.
- The scene, placeholder sprite, prefabs and theme were built in the open editor with a one-off script run through the Unity CLI, not by hand-editing YAML. A reference assigned to an asset in the same run was lost on save, and the smoke test caught it.
- Tests: 19 EditMode unit tests for `BoardLayout`, `SlotGrid` and `CellVisualCatalog`.

---

## Session 4 — 2026-11-09

### 17. PR 8 review
- **PR workflow.** After "review submitted", the AI replies to every comment with its plan or pushback. After "go", it fixes, then merges unless told otherwise. **Decision by:** you.
- **One `Grid<T>` in Sim** replaces the view-only `SlotGrid`. `Board` stores its cells in it, and `LayerView` stores its views in it. It lives in Sim, not a new shared assembly, because Sim is already the Unity-free assembly everything references. **Decision by:** you (AI chose Sim over a common project).
- **Renames toward standard terms.** `CellVisual` → `CellView`, `CellVisualCatalog` → `CellViewPool` (it pools, not offers options), `SpriteStateTint` → `CellStateSpriteTint`, `BoardLayout` → `BoardSpace` ("layout" suggests the board's shape), `Theme` → `CellTheme` (themes are modular, one per area). **Decision by:** you (AI proposed `BoardSpace`, `CellViewPool` and theme per area).
- **Sim config is an asset** (`SimConfigAsset`), and the sim uses the asset's live config, so tuning in play mode applies right away. That trades away reproducing a run from seed + inputs while tweaking. **Decision by:** you, aware of the determinism cost.
- **Edit-time gizmo previews** for anything sized only by config (board, hand slots), as a standard for WYSIWYG scenes. Plain gizmos rather than a paid drawing asset for now. **Decision by:** you.
- **Drag feel is prepared, not built.** Placement now reads a logical drag target, not the dragged view, so lag and tilt can be added in one method later. **Decision by:** you (AI found that the old code would skew placement once the view lagged).
- **Third-party assets.** Odin Inspector and ALINE were considered and dropped for now (`[ContextMenu]` and gizmos are enough). Paid assets in the repo would also need handling before a public showcase.
- **`Set` over `Show`** for methods that take data and display it. `Show` is reserved for visibility. The rule is in CLAUDE.md, and a follow-up refactor PR renamed `HandView.Show` to `SetHand` and `PieceView.Show` to `SetShape`.

### 18. Parallel agents and groundwork
- **Parallel AI agents.** Two background agents build separate features at once, each in a permanent git worktree with its own warm Unity `Library/` and a headless batch-mode Unity for import, compile and tests (about 90 s per check). Each agent owns its code and assets and opens small, stacked PRs. The main session pre-reviews with comments marked `[Claude]` before your review. **Decision by:** you (AI proposed batch mode and permanent slots).
- **One scene, screens as prefabs.** Scenes and self-starting MonoBehaviours hide the flow. Owners initialize what they own through explicit `Initialize(...)` calls, so app flow and UI read in code. **Decision by:** you.
- **`GameSetup` (config + seed) is the game's entry point**, so a future playtest screen can start any mode without touching the main game. **Decision by:** you (AI proposed the setup object).
- **Groundwork PR** before the agents start, so they don't both edit the same files: the Game prefab, `GameController.Initialize`, a temporary `GameBootstrap`, TMP essentials, EventSystem, a base `UiCanvas` (portrait 1080×1920) and an empty `GameHud` slot. The drag controller now gets the camera from its owner, since a prefab can't reference a scene camera.
- **uGUI + TextMeshPro** for UI. **Decision by:** you.
- **Effect handles** (start/stop VFX across cue steps) flagged for a proper design pass before playback builds them.

---

## Session 5 — 2026-11-10

### 19. AI workflow hardening
- **Implementer agent.** The parallel-agent workflow went from a per-task prompt to a defined `implementer` subagent: slot and branch hygiene, headless verification, a definition of done and a fixed PR description format (What / Decisions / Verified / Needs the editor). The three agents (`implementer`, `reviewer`, `workflow-reviewer`) are now tracked in git. **Decision by:** both (you asked whether implementers were defined, AI proposed the agent).
- **Prefab builder scripts are throwaway.** Agents build prefabs with a batch-mode editor script, commit the asset and delete the script, because hand edits afterwards would make a kept builder stale. **Decision by:** AI, agreed by you.
- **Workflow review** by the `workflow-reviewer` agent. Its main finding was that the system wasn't visible: rules sat in local AI memory, and guardrails were prose, not enforced. Acting on it:
  - **Project rules moved into CLAUDE.md** (tests, doc comments, PR review loop, `TODO.txt`, worktree slots), so subagents and readers of the repo see them. Memory keeps only personal preferences. **Decision by:** you.
  - **`Tools/verify.sh`**: one script for dotnet test plus Unity batch import, compile and EditMode tests. It prints only errors and totals, reads the Unity version from the project, and refuses a project open in the editor. A full run on a slot takes about 40 s. **Decision by:** both.
  - **CI:** GitHub Actions runs the Sim tests on every PR and push to `main`. **Decision by:** you.
  - **Enforced guardrails:** a hook blocks `unity command` from subagents (tested with a real subagent), and `git reset --hard` and force pushes always prompt. **Decision by:** you (AI corrected the reviewer's suggested deny path and hook trigger).
- **README deferred** until the project is closer to releasable. **Decision by:** you.

---

## Highlights (raw material for README / CV)

- Designed a **deterministic, headless puzzle simulation** in C#, decoupled from Unity, with full unit-test coverage running in both .NET and Unity.
- Built for **live ops from day one**: data-driven, serializable game modes; a rule factory for pluggable dealers, scoring and win/lose conditions; and a generic **board pattern system** that turns new puzzle goals into data.
- **Command/event architecture**: replayable command objects and an ordered event stream for animation, analytics and meta systems.
- **Defensive API design**: read-only views over sim state and a hidden RNG, so the presentation layer can't break determinism.
- **AI-assisted development with engineering discipline**: a written AI working agreement, explicit style and architecture rules, and PR-based review of all AI-generated code, with architectural feedback driving the design.
- **Multi-agent pipeline with enforced guardrails**: implementer and reviewer subagents working in parallel worktrees, a headless Unity verification script, CI, and hooks that keep agents from touching the developer's open editor.
