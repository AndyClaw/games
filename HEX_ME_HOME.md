# Hex Me Home — Game Specification

## Overview

Hex Me Home is a puzzle game played on a hexagonal tile grid. The player rotates tiles to create a continuous path from the Player marker (top edge) to the Home marker (bottom edge). Each tile has three paths connecting pairs of its six sides. Rotating a tile cycles it 60° clockwise, changing which sides connect to adjacent tiles.

## Grid Structure

### Hex Tile Layout
- Tiles are regular hexagons arranged in an offset grid (pointy-top hexagons, odd rows shifted right).
- Grid sizes: **4×4** (default on page load), **6×6**, **8×8** — selectable via buttons.
- The grid is rectangular in shape. Because hexagons don't tile into a perfect rectangle, **half-hex edge pieces** fill the gaps along the top and bottom edges.

### Half-Hex Edge Pieces
- These are non-interactive triangular/half-hex fillers along the top and bottom borders.
- They serve as attachment points for the Player start marker and Home marker.
- Each half-hex edge piece has one "open side" that connects to the adjacent full tile.

### Coordinate System
- Column (x): 0-based, left to right.
- Row (y): 0-based, top to bottom.
- Odd rows are offset to the right by half a hex width.

## Tile Anatomy

### Six Sides
Sides are numbered 0–5, starting from the top-right and going clockwise:
```
     5   0
    /     \
   4       1
    \     /
     3   2
```
- Side 0: top-right (between top vertex and upper-right vertex)
- Side 1: right (between upper-right vertex and lower-right vertex)
- Side 2: bottom-right (between lower-right vertex and bottom vertex)
- Side 3: bottom-left (between bottom vertex and lower-left vertex)
- Side 4: left (between lower-left vertex and upper-left vertex)
- Side 5: top-left (between upper-left vertex and top vertex)

This is a **pointy-top** hex orientation (vertex at top and bottom, flat edges on left and right).

### Three Paths Per Tile
Each tile contains exactly 3 paths. Each path connects two distinct sides. All 6 sides are used exactly once (forming a perfect matching of the 6 sides into 3 pairs).

There are 15 possible pairings of 6 sides into 3 pairs. All 15 tile types are valid for random generation.

### Rotation
- Clicking a tile rotates it **60° clockwise** (one step).
- Rotation shifts all path connections: a path connecting sides (a, b) becomes ((a+1)%6, (b+1)%6).
- After 6 clicks, a tile returns to its original orientation.
- Rotation is instant (no animation).

### Adjacency
Two tiles are adjacent if they share an edge. The side of tile A that faces tile B is the **complement** of the side of tile B that faces tile A.

General rule: Side `s` of a tile connects to side `(s + 3) % 6` of the adjacent tile on that side.

Neighbor coordinates use **odd-row offset** (odd rows shifted right):

For **even rows** (row % 2 == 0):
| Exit Side | Neighbor (col, row) |
|-----------|-------------------|
| 0 (top-right) | (col, row-1) |
| 1 (right) | (col+1, row) |
| 2 (bottom-right) | (col, row+1) |
| 3 (bottom-left) | (col-1, row+1) |
| 4 (left) | (col-1, row) |
| 5 (top-left) | (col-1, row-1) |

For **odd rows** (row % 2 == 1):
| Exit Side | Neighbor (col, row) |
|-----------|-------------------|
| 0 (top-right) | (col+1, row-1) |
| 1 (right) | (col+1, row) |
| 2 (bottom-right) | (col+1, row+1) |
| 3 (bottom-left) | (col, row+1) |
| 4 (left) | (col-1, row) |
| 5 (top-left) | (col, row-1) |

## Players and Goals

### Player Marker
- Located on the **top edge** of the grid (attached to a half-hex edge piece or the top side of a top-row tile).
- Represented by a colored marker. First player color: **red**.
- Future expansion: multiple players (green, blue) with corresponding homes.

### Home Marker
- Located on the **bottom edge** of the grid.
- Same color as its corresponding player.
- The player must connect to their matching-color home.

### Placement Rules
- Player always on top edge, Home always on bottom edge.
- For a given level, a random column is chosen for the Player entry point, and a random column for the Home entry point.
- The Player enters through side 5 (top-left) of the first-row tile in the chosen column.
- The Home is reached by exiting through side 3 (bottom-left) of the last-row tile in the chosen column.

## Level Generation Algorithm

1. **Place Player and Home**: Choose random column for Player (top edge) and random column for Home (bottom edge).

2. **Find solution path**: Starting from the Player's entry tile/side, perform a random walk through the grid:
   - At each tile, the entry side is known (from the previous tile or from the Player marker).
   - Randomly choose one of the other 5 sides as the exit side for the first path on this tile.
   - The exit side determines which adjacent tile is next.
   - Continue until reaching the Home marker's tile and exiting through the Home side.
   - If the walk gets stuck (leaves the grid, revisits a tile, or can't reach Home), restart generation.

3. **Assign paths to solution tiles**: For each tile on the solution path:
   - The first path pair is the (entry side, exit side) determined in step 2.
   - Randomly pair the remaining 4 sides into 2 more paths.

4. **Assign paths to non-solution tiles**: For tiles not on the solution path:
   - Randomly generate all 3 path pairs (random perfect matching of 6 sides into 3 pairs).

5. **Store the solution**: Save each tile's solution rotation (orientation index 0–5).

6. **Scramble**: Rotate each solution-path tile randomly 1–5 times (never 0, so it's always displaced from solution). Non-solution tiles are also randomly rotated 0–5 times.

## Win Condition Check

After every tile rotation:

1. Start at the Player marker's entry point (known tile and entry side).
2. On the current tile, find which path includes the entry side. The other side of that path is the exit side.
3. Determine the adjacent tile connected via the exit side.
4. If the adjacent tile is the Home marker → **player wins**.
5. If the adjacent tile is off-grid, a half-hex edge (not Home), or already visited → path is a dead end, no win.
6. Otherwise, enter the adjacent tile on the complementary side and repeat from step 2.
7. Maximum traversal length = total number of tiles (prevents infinite loops on cyclic paths).

**Note**: Multiple valid solutions may exist beyond the generated one. Any path connecting Player to Home counts as a win.

## UI Controls

Three buttons:
1. **Reset** — Restore all tiles to their scrambled state from the start of the current level (undo all player rotations).
2. **New Game** — Generate a completely new random level at the current grid size.
3. **Show Solution** — Instantly snap all tiles to their solution orientation. Highlight solution-path tiles with a colored border/glow matching the player color.

Grid size buttons: **4×4**, **6×6**, **8×8** — generating a new game at that size.

## Path Highlighting (Solve Check Visualization)

After each tile rotation, when the solve check traverses the path from the Player:
- All tiles/paths traversed during the check are highlighted with the player's color (red).
- The highlight shows exactly how far the current path goes before it dead-ends or reaches Home.
- Non-traversed paths remain in their default white color.
- This gives the player visual feedback on their progress.

## Rendering

### Tile Rendering
- Each tile is rendered as an SVG hexagon.
- Background fill: light gray (#808080 or similar).
- Paths: drawn as arcs connecting the midpoints of two sides.
  - Each path is drawn twice: first a slightly wider black stroke (border), then a white stroke on top.
  - Paths are drawn in order so overlapping arcs layer correctly.
- Rotation is applied via SVG `rotate()` transform (rotation_index × 60°).

### Active Path Rendering
- When a path segment is part of the current traversal from Player, the white stroke is replaced with the player's color (red).

### Solution Highlight
- When "Show Solution" is active, solution-path tiles get a colored border/outline around the hexagon edge.

### Half-Hex Edge Pieces
- Rendered as half-hexagons (top or bottom halves) in a lighter shade.
- Player marker: filled/outlined in player color.
- Home marker: filled/outlined in player color with a distinct icon or "H" label.

## Page Route

`/hexmehome`

## Future Expansion (Not in v1)

- Multiple players (red, green, blue) each needing their own path to their matching home.
- All players must be connected simultaneously for a win.
- Timed mode / move counter.
- Predefined puzzle packs.




