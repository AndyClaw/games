using BlazorApp1.Models;

namespace BlazorApp1.Services;

/// <summary>
/// Game engine for Hex Me Home. Handles level generation, rotation, and win checking.
/// </summary>
public class HexMeHomeEngine
{
    private static readonly Random _rng = new();

    public HexGrid Grid { get; private set; } = null!;

    /// <summary>
    /// Stored initial scramble state for reset.
    /// Key: (col, row), Value: rotation at start of level.
    /// </summary>
    private Dictionary<(int, int), int> _initialRotations = new();

    public event Action? OnStateChanged;

    /// <summary>
    /// Generate a new game with the given grid size.
    /// </summary>
    public void NewGame(int columns, int rows)
    {
        Grid = new HexGrid
        {
            Columns = columns,
            Rows = rows,
            Tiles = new HexTile[columns, rows]
        };

        // Initialize all tiles
        for (int c = 0; c < columns; c++)
        for (int r = 0; r < rows; r++)
        {
            Grid.Tiles[c, r] = new HexTile { Col = c, Row = r };
        }

        GenerateLevel();
        OnStateChanged?.Invoke();
    }

    private void GenerateLevel()
    {
        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (TryGenerateLevel())
            {
                StoreInitialState();
                return;
            }
        }
        // Fallback: just generate random tiles with no guaranteed solution
        GenerateRandomTilesOnly();
        StoreInitialState();
    }

    private bool TryGenerateLevel()
    {
        int cols = Grid.Columns;
        int rows = Grid.Rows;

        // 1. Place player (top) and home (bottom)
        Grid.PlayerColumn = _rng.Next(cols);
        Grid.HomeColumn = _rng.Next(cols);

        // Player enters from top. For pointy-top hexes, top-facing sides are 0 (top-right) and 5 (top-left).
        // We'll use side 5 so the path enters from upper-left of the tile.
        Grid.PlayerEntrySide = 5;

        // Home exits from bottom. Bottom-facing sides are 2 (bottom-right) and 3 (bottom-left).
        Grid.HomeExitSide = 3;

        // 2. Find a solution path via random walk
        var solutionPath = FindRandomPath();
        if (solutionPath == null) return false;

        // 3. Assign paths to solution tiles
        var solutionTileSet = new HashSet<(int, int)>();
        for (int i = 0; i < solutionPath.Count; i++)
        {
            var (col, row, entrySide, exitSide) = solutionPath[i];
            solutionTileSet.Add((col, row));
            var tile = Grid.Tiles[col, row];
            tile.IsSolutionTile = true;

            // First path is the solution connection (in base rotation, so sides are as-is)
            // We'll build paths in base rotation (rotation=0), then set solution rotation=0
            AssignTilePaths(tile, entrySide, exitSide);
        }

        // 4. Assign paths to non-solution tiles
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            if (!solutionTileSet.Contains((c, r)))
            {
                AssignRandomPaths(Grid.Tiles[c, r]);
            }
        }

        // 5. Store solution rotations (all at 0 since we built paths in correct orientation)
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            Grid.Tiles[c, r].SolutionRotation = 0;
        }

        // 6. Scramble
        foreach (var (col, row, _, _) in solutionPath)
        {
            int rotations = _rng.Next(1, 6); // 1-5, never 0
            Grid.Tiles[col, row].Rotation = rotations;
        }
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            if (!solutionTileSet.Contains((c, r)))
            {
                Grid.Tiles[c, r].Rotation = _rng.Next(0, 6);
            }
        }

        Grid.IsSolved = false;
        Grid.ShowingSolution = false;
        CheckWin(); // Sets traversal highlights
        return true;
    }

    /// <summary>
    /// Attempts to find a random path from player entry to home exit.
    /// Returns list of (col, row, entrySide, exitSide) for each tile on the path.
    /// </summary>
    private List<(int Col, int Row, int EntrySide, int ExitSide)>? FindRandomPath()
    {
        var path = new List<(int Col, int Row, int EntrySide, int ExitSide)>();
        var visited = new HashSet<(int, int)>();

        int currentCol = Grid.PlayerColumn;
        int currentRow = 0;
        int entrySide = Grid.PlayerEntrySide;

        int maxSteps = Grid.Columns * Grid.Rows;

        for (int step = 0; step < maxSteps; step++)
        {
            if (currentCol < 0 || currentCol >= Grid.Columns ||
                currentRow < 0 || currentRow >= Grid.Rows)
                return null;

            if (visited.Contains((currentCol, currentRow)))
                return null;

            visited.Add((currentCol, currentRow));

            // Check if we can exit to home from this tile
            if (currentRow == Grid.Rows - 1 && currentCol == Grid.HomeColumn)
            {
                // Can we exit through the home side?
                int homeExitSide = Grid.HomeExitSide;
                if (homeExitSide != entrySide) // Don't go back the way we came
                {
                    path.Add((currentCol, currentRow, entrySide, homeExitSide));
                    return path;
                }
            }

            // Choose a random exit side (not the entry side)
            var possibleExits = GetValidExitSides(currentCol, currentRow, entrySide, visited);

            // Also allow home exit if we're on the home tile
            if (currentRow == Grid.Rows - 1 && currentCol == Grid.HomeColumn)
            {
                if (!possibleExits.Contains(Grid.HomeExitSide) && Grid.HomeExitSide != entrySide)
                    possibleExits.Add(Grid.HomeExitSide);
            }

            if (possibleExits.Count == 0)
                return null;

            int exitSide = possibleExits[_rng.Next(possibleExits.Count)];
            path.Add((currentCol, currentRow, entrySide, exitSide));

            // Move to next tile
            var (nextCol, nextRow) = GetNeighbor(currentCol, currentRow, exitSide);
            entrySide = (exitSide + 3) % 6; // Complementary side

            // Check if we just exited to home
            if (currentRow == Grid.Rows - 1 && currentCol == Grid.HomeColumn && exitSide == Grid.HomeExitSide)
            {
                return path;
            }

            currentCol = nextCol;
            currentRow = nextRow;
        }

        return null;
    }

    private List<int> GetValidExitSides(int col, int row, int entrySide, HashSet<(int, int)> visited)
    {
        var valid = new List<int>();
        for (int side = 0; side < 6; side++)
        {
            if (side == entrySide) continue;
            var (nCol, nRow) = GetNeighbor(col, row, side);

            // Allow exit to home
            if (row == Grid.Rows - 1 && col == Grid.HomeColumn && side == Grid.HomeExitSide)
            {
                valid.Add(side);
                continue;
            }

            // Must lead to a valid unvisited tile
            if (nCol >= 0 && nCol < Grid.Columns && nRow >= 0 && nRow < Grid.Rows && !visited.Contains((nCol, nRow)))
            {
                valid.Add(side);
            }
        }
        return valid;
    }

    /// <summary>
    /// Get the neighbor tile coordinates when exiting through a given side.
    /// Uses pointy-top hexagons with odd-row offset (odd rows shifted right).
    /// Side numbering: 0=top-right, 1=right, 2=bottom-right, 3=bottom-left, 4=left, 5=top-left
    /// </summary>
    public static (int Col, int Row) GetNeighbor(int col, int row, int exitSide)
    {
        bool isOddRow = row % 2 == 1;

        return exitSide switch
        {
            0 => isOddRow ? (col + 1, row - 1) : (col, row - 1),     // top-right
            1 => (col + 1, row),                                       // right
            2 => isOddRow ? (col + 1, row + 1) : (col, row + 1),     // bottom-right
            3 => isOddRow ? (col, row + 1) : (col - 1, row + 1),     // bottom-left
            4 => (col - 1, row),                                       // left
            5 => isOddRow ? (col, row - 1) : (col - 1, row - 1),     // top-left
            _ => (-1, -1)
        };
    }

    /// <summary>
    /// Assign paths to a solution tile given the required entry-exit connection.
    /// </summary>
    private void AssignTilePaths(HexTile tile, int entrySide, int exitSide)
    {
        tile.Paths[0] = (entrySide, exitSide);

        // Remaining 4 sides need to be paired into 2 paths
        var remaining = new List<int>();
        for (int s = 0; s < 6; s++)
        {
            if (s != entrySide && s != exitSide)
                remaining.Add(s);
        }

        // Shuffle and pair
        Shuffle(remaining);
        tile.Paths[1] = (remaining[0], remaining[1]);
        tile.Paths[2] = (remaining[2], remaining[3]);
    }

    /// <summary>
    /// Assign completely random paths to a tile.
    /// </summary>
    private void AssignRandomPaths(HexTile tile)
    {
        var sides = new List<int> { 0, 1, 2, 3, 4, 5 };
        Shuffle(sides);
        tile.Paths[0] = (sides[0], sides[1]);
        tile.Paths[1] = (sides[2], sides[3]);
        tile.Paths[2] = (sides[4], sides[5]);
    }

    private void GenerateRandomTilesOnly()
    {
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            AssignRandomPaths(Grid.Tiles[c, r]);
            Grid.Tiles[c, r].Rotation = _rng.Next(0, 6);
        }
    }

    private void StoreInitialState()
    {
        _initialRotations.Clear();
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            _initialRotations[(c, r)] = Grid.Tiles[c, r].Rotation;
        }
    }

    /// <summary>
    /// Rotate a tile and recheck win condition.
    /// </summary>
    public void RotateTile(int col, int row)
    {
        if (Grid.IsSolved || Grid.ShowingSolution) return;

        Grid.Tiles[col, row].Rotate();
        CheckWin();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Reset all tiles to initial scrambled state.
    /// </summary>
    public void Reset()
    {
        Grid.ShowingSolution = false;
        Grid.IsSolved = false;
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            Grid.Tiles[c, r].Rotation = _initialRotations[(c, r)];
        }
        CheckWin();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Show the solution by snapping all tiles to solution rotation.
    /// </summary>
    public void ShowSolution()
    {
        Grid.ShowingSolution = true;
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            Grid.Tiles[c, r].Rotation = Grid.Tiles[c, r].SolutionRotation;
        }
        CheckWin();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Traverse the path from player entry and mark traversed tiles/paths.
    /// Sets IsSolved if path reaches home.
    /// A tile can be visited multiple times using different paths (overpass crossings).
    /// </summary>
    public void CheckWin()
    {
        // Clear traversal state
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            Grid.Tiles[c, r].IsTraversed = false;
            Grid.Tiles[c, r].TraversedPathIndices.Clear();
        }

        int currentCol = Grid.PlayerColumn;
        int currentRow = 0;
        int entrySide = Grid.PlayerEntrySide;
        var visited = new HashSet<(int, int, int)>(); // (col, row, pathIndex)
        int maxSteps = Grid.Columns * Grid.Rows * 3; // max = every path on every tile

        for (int step = 0; step < maxSteps; step++)
        {
            if (currentCol < 0 || currentCol >= Grid.Columns ||
                currentRow < 0 || currentRow >= Grid.Rows)
            {
                Grid.IsSolved = false;
                return;
            }

            var tile = Grid.Tiles[currentCol, currentRow];
            int pathIndex = tile.GetPathIndexForSide(entrySide);
            if (pathIndex == -1)
            {
                Grid.IsSolved = false;
                return;
            }

            // Check if we've already used this specific path on this tile (loop detection)
            if (visited.Contains((currentCol, currentRow, pathIndex)))
            {
                Grid.IsSolved = false;
                return;
            }

            visited.Add((currentCol, currentRow, pathIndex));

            int exitSide = tile.GetExitSide(entrySide);
            if (exitSide == -1)
            {
                Grid.IsSolved = false;
                return;
            }

            // Mark this tile and path as traversed
            tile.IsTraversed = true;
            tile.TraversedPathIndices.Add(pathIndex);

            // Check if we reached home
            if (currentRow == Grid.Rows - 1 && currentCol == Grid.HomeColumn && exitSide == Grid.HomeExitSide)
            {
                Grid.IsSolved = true;
                return;
            }

            // Move to next
            var (nextCol, nextRow) = GetNeighbor(currentCol, currentRow, exitSide);
            entrySide = (exitSide + 3) % 6;
            currentCol = nextCol;
            currentRow = nextRow;
        }

        Grid.IsSolved = false;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}



