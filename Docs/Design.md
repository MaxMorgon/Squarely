# Squarely — Design

A block-placement puzzle game (genre of 1010!, Block Blast), built as a learning project that can grow into a live-ops product. Core rules follow the genre. Art, UI, name and meta features are our own.

## Core rules (v1)

### Board
- Square grid, **8×8**. Each cell is empty or filled.

### Pieces
- Pieces are polyominoes (a single cell up to e.g. a 3×3 square, lines up to 5, L/T/S/Z shapes).
- Pieces **cannot be rotated**.
- The player has a **hand of 3 pieces** (configurable) and places them in any order.
- The hand refills only when it is empty: after all 3 pieces are placed, a full new hand is dealt.

### Dealing
- Pieces are dealt at random, weighted by each piece's weight in config.
- **Helpful piece guarantee** (config switch, on in the classic mode): every new hand holds at least one *helpful* piece for the board at that moment. A helpful piece can complete a pattern somewhere. If no piece in the pool can complete a pattern, a helpful piece is a piece that fits somewhere. If the weighted hand has none, one random slot is redealt from the helpful pieces only, still by weight. If no piece in the pool fits at all, the hand is dealt as normal and the game is lost as usual.
- Each piece is judged alone on the current board, not together with the rest of the hand.

### Placement
- A piece can go anywhere it fits entirely on empty cells inside the board.
- After each placement, every completed **board pattern** clears at the same time (a cell in several matches clears once).
- By default the patterns are the **full rows and columns**. Modes can add custom patterns: pixel-art images, small shapes like a smiley, abstract shapes. A pattern can be anchored to a spot or match anywhere, and has its own points. A completed image could be "tattooed" into the background.

### Scoring
- Placing a piece: +1 point per cell placed.
- Completing patterns: each pattern's own points (lines have a default), plus a bonus for completing several in one placement.
- **Combo/streak:** consecutive placements that complete patterns raise a multiplier. A placement with no clear resets it.
- Exact numbers live in config, not code.

### Game resolution
- A game resolves as **won** or **lost**, decided by configurable resolution rules.
- Default: **lost** when no piece in the hand fits anywhere ("no moves left"). Other modes can add rules such as a target score (won) or a move limit.

## Live-ops goals

New content and features must be cheap to ship:

- **Data-driven config.** Board size, piece set, piece weights, scoring values and hand size are data, not constants. Remote delivery may come later.
- **Extensible rules.** New mechanics (special cells, blockers, power-ups, objectives) plug into the sim through defined hooks, not by editing the core loop.
- **Event output.** The sim emits events (placed, cleared, scored, combo, resolved) for the Unity layer, analytics and meta systems.
- **Meta layer separate from core.** Progression, currencies, quests and events sit outside the puzzle sim and react to its events.
- **Deterministic and seeded.** This enables replays, bug reproduction, daily challenges with a shared seed, and server validation later.

## Open questions

- Dealing is decided for now (see [Dealing](#dealing)). Open: tune it further (e.g. a chance instead of always, or judging the hand as a whole).
- Exact scoring numbers and combo rules.
- Undo / continue / revive in v1?
- Game modes: endless only at first, or also level/objective modes?

## Game mode ideas

- **Picture puzzle.** The board shows an image in the background. The player gets a fixed set of pieces (about 6 to 10). The player places them to complete the image. A short, hand-made level.

## Roadmap (rough)

1. Sim: board, pieces, placement, line clears, scoring, game over, fully tested.
2. Unity: render board and hand, drag-and-drop placement, basic feedback.
3. Config: move all tunables into data.
4. Meta features and live-ops infrastructure.
