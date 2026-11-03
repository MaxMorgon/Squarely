# Squarely — Design

A block-placement puzzle game (the genre of 1010!, Block Blast, etc.), built as a learning project and intended to grow into a live-ops product. Core rules follow the genre; art, UI, name and meta features are our own.

## Core rules (v1)

### Board
- Square grid, **8×8**. Each cell is empty or filled.

### Pieces
- Pieces are polyomino shapes (single cell up to e.g. 3×3 square, lines up to 5, L/T/S/Z shapes).
- Pieces **cannot be rotated**.
- The player has a **hand of 3 pieces** (configurable). They can be placed in any order.
- The hand is always kept full: after each placement, the empty slot is refilled immediately.

### Placement
- A piece can be placed anywhere it fits entirely on empty cells inside the board.
- After each placement, every completed **board pattern** is cleared simultaneously (a cell in several matches is cleared once).
- By default the patterns are the **full rows and columns**. Modes can add custom patterns: pixel-art images, small shapes like a smiley, abstract shapes. They can be anchored to a spot or match anywhere, and each is worth its own points. A completed image could be "tattooed" into the background.

### Scoring
- Placing a piece: +1 point per cell placed.
- Completing patterns: each pattern's own points (lines have a default value), plus a bonus for completing several in one placement.
- **Combo/streak:** consecutive placements that complete patterns increase a multiplier; a placement with no clear resets it.
- Exact numbers live in config, not code.

### Game resolution
- A game resolves as **won** or **lost**, decided by configurable resolution rules.
- Default: **lost** when none of the pieces in the hand fit anywhere on the board ("no moves left"). Other modes can add rules such as reaching a target score (won) or a move limit.

## Live-ops goals

The product should make new content and features cheap to ship:

- **Data-driven config** — board size, piece set, piece weights, scoring values, hand size are data, not constants. Can later be delivered remotely.
- **Extensible rules** — new mechanics (special cells, blockers, power-ups, objectives) plug into the sim through well-defined hooks rather than editing the core loop.
- **Event output** — the sim emits events (placed, cleared, scored, combo, resolved) that the Unity layer, analytics, and meta systems consume.
- **Meta layer separate from core** — progression, currencies, quests, events sit outside the core puzzle sim and react to its events.
- **Deterministic + seeded** — enables replays, bug reproduction, daily challenges with a shared seed, and server validation later.

## Open questions

- Piece dealing: purely random by weight, or "fair" dealing that guarantees at least one piece fits?
- Exact scoring numbers and combo rules.
- Any undo / continue / revive mechanic in v1?
- Game modes: endless only at first, or also level/objective modes?

## Roadmap (rough)

1. Sim: board, pieces, placement, line clears, scoring, game over — fully tested.
2. Unity: render board and hand, drag-and-drop placement, basic feedback.
3. Config: move all tunables into data.
4. Meta features and live-ops infrastructure.
