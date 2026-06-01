using BlazorApp1.Models;

namespace BlazorApp1.Services;

/// <summary>
/// Game engine for Hex Me Home. Handles level generation, rotation, and win checking.
/// </summary>
public class HexMeHomeEngine
{
    private static readonly Random _rng = new();
    private static readonly string[] PlayerColors = { "#e63946", "#2d6a4f", "#1d3557" };

    public HexGrid Grid { get; private set; } = null!;

    /// <summary>
    /// Number of players for the current game.
    /// </summary>
    public int PlayerCount { get; private set; } = 1;

    /// <summary>
    /// Stored initial scramble state for reset.
    /// </summary>
    private Dictionary<(int, int), int> _initialRotations = new();

    public event Action? OnStateChanged;

    /// <summary>
    /// Generate a new game with the given grid size and current player count.
    /// </summary>
    public void NewGame(int columns, int rows)
    {
        Grid = new HexGrid
        {
            Columns = columns,
            Rows = rows,
            Tiles = new HexTile[columns, rows]
        };

        for (int c = 0; c < columns; c++)
        for (int r = 0; r < rows; r++)
        {
            Grid.Tiles[c, r] = new HexTile { Col = c, Row = r };
        }

        GenerateLevel();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Add a player (up to 3) and regenerate the level.
    /// </summary>
    public void AddPlayer(int currentGridSize)
    {
        if (PlayerCount >= 3) return;
        PlayerCount++;
        NewGame(currentGridSize, currentGridSize);
    }

    /// <summary>
    /// Remove a player (minimum 1) and regenerate.
    /// </summary>
    public void RemovePlayer(int currentGridSize)
    {
        if (PlayerCount <= 1) return;
        PlayerCount--;
        NewGame(currentGridSize, currentGridSize);
    }

    private void GenerateLevel()
    {
        int maxAttempts = 200;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (TryGenerateLevel())
            {
                StoreInitialState();
                return;
            }
        }
        // Fallback
        GenerateRandomTilesOnly();
        StoreInitialState();
    }

    private bool TryGenerateLevel()
    {
        int cols = Grid.Columns;
        int rows = Grid.Rows;

        // Clear any prior state
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            Grid.Tiles[c, r].IsSolutionTile = false;
        }

        // Generate endpoints - each player gets a unique column for start and end
        Grid.Endpoints.Clear();
        var usedTopCols = new HashSet<int>();
        var usedBottomCols = new HashSet<int>();

        for (int p = 0; p < PlayerCount; p++)
        {
            int playerCol, homeCol;
            // Pick unique columns
            do { playerCol = _rng.Next(cols); } while (usedTopCols.Contains(playerCol));
            do { homeCol = _rng.Next(cols); } while (usedBottomCols.Contains(homeCol));
            usedTopCols.Add(playerCol);
            usedBottomCols.Add(homeCol);

            Grid.Endpoints.Add(new PlayerEndpoint
            {
                PlayerColumn = playerCol,
                PlayerEntrySide = 5,
                HomeColumn = homeCol,
                HomeExitSide = 3,
                Color = PlayerColors[p]
            });
        }

        // Generate solution paths for each endpoint
        var allSolutionPaths = new List<List<(int Col, int Row, int EntrySide, int ExitSide)>>();
        var solutionTileSet = new HashSet<(int, int)>();

        for (int p = 0; p < PlayerCount; p++)
        {
            var endpoint = Grid.Endpoints[p];
            var path = FindRandomPath(endpoint, solutionTileSet);
            if (path == null) return false;
            allSolutionPaths.Add(path);

            foreach (var (col, row, _, _) in path)
                solutionTileSet.Add((col, row));
        }

        // Assign paths to solution tiles
        foreach (var solutionPath in allSolutionPaths)
        {
            for (int i = 0; i < solutionPath.Count; i++)
            {
                var (col, row, entrySide, exitSide) = solutionPath[i];
                var tile = Grid.Tiles[col, row];
                tile.IsSolutionTile = true;
                AssignTilePaths(tile, entrySide, exitSide);
            }
        }

        // Assign paths to non-solution tiles
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            if (!solutionTileSet.Contains((c, r)))
            {
                AssignRandomPaths(Grid.Tiles[c, r]);
            }
        }

        // Store solution rotations
        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            Grid.Tiles[c, r].SolutionRotation = 0;
        }

        // Scramble solution tiles
        foreach (var solutionPath in allSolutionPaths)
        {
            foreach (var (col, row, _, _) in solutionPath)
            {
                int rotations = _rng.Next(1, 6);
                Grid.Tiles[col, row].Rotation = rotations;
            }
        }
        // Scramble non-solution tiles
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
        CheckWin();
        return true;
    }

    /// <summary>
    /// Find a random path for a specific endpoint, avoiding tiles already used by other solutions.
    /// </summary>
    private List<(int Col, int Row, int EntrySide, int ExitSide)>? FindRandomPath(
        PlayerEndpoint endpoint, HashSet<(int, int)> reservedTiles)
    {
        var path = new List<(int Col, int Row, int EntrySide, int ExitSide)>();
        var visited = new HashSet<(int, int)>(reservedTiles);

        int currentCol = endpoint.PlayerColumn;
        int currentRow = 0;
        int entrySide = endpoint.PlayerEntrySide;

        int maxSteps = Grid.Columns * Grid.Rows;

        for (int step = 0; step < maxSteps; step++)
        {
            if (currentCol < 0 || currentCol >= Grid.Columns ||
                currentRow < 0 || currentRow >= Grid.Rows)
                return null;

            if (visited.Contains((currentCol, currentRow)))
                return null;

            visited.Add((currentCol, currentRow));

            // Check if we can exit to home
            if (currentRow == Grid.Rows - 1 && currentCol == endpoint.HomeColumn)
            {
                int homeExitSide = endpoint.HomeExitSide;
                if (homeExitSide != entrySide)
                {
                    path.Add((currentCol, currentRow, entrySide, homeExitSide));
                    return path;
                }
            }

            // Choose a random exit side
            var possibleExits = GetValidExitSides(currentCol, currentRow, entrySide, visited, endpoint);

            if (currentRow == Grid.Rows - 1 && currentCol == endpoint.HomeColumn)
            {
                if (!possibleExits.Contains(endpoint.HomeExitSide) && endpoint.HomeExitSide != entrySide)
                    possibleExits.Add(endpoint.HomeExitSide);
            }

            if (possibleExits.Count == 0)
                return null;

            int exitSide = possibleExits[_rng.Next(possibleExits.Count)];
            path.Add((currentCol, currentRow, entrySide, exitSide));

            // Check if we just exited to home
            if (currentRow == Grid.Rows - 1 && currentCol == endpoint.HomeColumn && exitSide == endpoint.HomeExitSide)
                return path;

            var (nextCol, nextRow) = GetNeighbor(currentCol, currentRow, exitSide);
            entrySide = (exitSide + 3) % 6;
            currentCol = nextCol;
            currentRow = nextRow;
        }

        return null;
    }

    private List<int> GetValidExitSides(int col, int row, int entrySide,
        HashSet<(int, int)> visited, PlayerEndpoint endpoint)
    {
        var valid = new List<int>();
        for (int side = 0; side < 6; side++)
        {
            if (side == entrySide) continue;
            var (nCol, nRow) = GetNeighbor(col, row, side);

            if (row == Grid.Rows - 1 && col == endpoint.HomeColumn && side == endpoint.HomeExitSide)
            {
                valid.Add(side);
                continue;
            }

            if (nCol >= 0 && nCol < Grid.Columns && nRow >= 0 && nRow < Grid.Rows && !visited.Contains((nCol, nRow)))
            {
                valid.Add(side);
            }
        }
        return valid;
    }

    /// <summary>
    /// Get the neighbor tile coordinates when exiting through a given side.
    /// </summary>
    public static (int Col, int Row) GetNeighbor(int col, int row, int exitSide)
    {
        bool isOddRow = row % 2 == 1;

        return exitSide switch
        {
            0 => isOddRow ? (col + 1, row - 1) : (col, row - 1),
            1 => (col + 1, row),
            2 => isOddRow ? (col + 1, row + 1) : (col, row + 1),
            3 => isOddRow ? (col, row + 1) : (col - 1, row + 1),
            4 => (col - 1, row),
            5 => isOddRow ? (col, row - 1) : (col - 1, row - 1),
            _ => (-1, -1)
        };
    }

    private void AssignTilePaths(HexTile tile, int entrySide, int exitSide)
    {
        tile.Paths[0] = (entrySide, exitSide);

        var remaining = new List<int>();
        for (int s = 0; s < 6; s++)
        {
            if (s != entrySide && s != exitSide)
                remaining.Add(s);
        }

        Shuffle(remaining);
        tile.Paths[1] = (remaining[0], remaining[1]);
        tile.Paths[2] = (remaining[2], remaining[3]);
    }

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
        Grid.Endpoints.Clear();
        Grid.Endpoints.Add(new PlayerEndpoint
        {
            PlayerColumn = _rng.Next(Grid.Columns),
            HomeColumn = _rng.Next(Grid.Columns),
            Color = PlayerColors[0]
        });
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

    public void RotateTile(int col, int row)
    {
        if (Grid.IsSolved || Grid.ShowingSolution) return;
        Grid.Tiles[col, row].Rotate();
        CheckWin();
        OnStateChanged?.Invoke();
    }

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
    /// Check all player paths. All must connect for a win.
    /// Each player's traversal is tracked with their color.
    /// </summary>
    public void CheckWin()
    {
        // Clear traversal state
        for (int c = 0; c < Grid.Columns; c++)
        for (int r = 0; r < Grid.Rows; r++)
        {
            Grid.Tiles[c, r].IsTraversed = false;
            Grid.Tiles[c, r].TraversedPaths.Clear();
        }

        bool allSolved = true;

        foreach (var endpoint in Grid.Endpoints)
        {
            endpoint.IsSolved = TraversePath(endpoint);
            if (!endpoint.IsSolved) allSolved = false;
        }

        Grid.IsSolved = allSolved;
    }

    /// <summary>
    /// Traverse a single player's path and mark tiles with their color.
    /// </summary>
    private bool TraversePath(PlayerEndpoint endpoint)
    {
        int currentCol = endpoint.PlayerColumn;
        int currentRow = 0;
        int entrySide = endpoint.PlayerEntrySide;
        var visited = new HashSet<(int, int, int)>();
        int maxSteps = Grid.Columns * Grid.Rows * 3;

        for (int step = 0; step < maxSteps; step++)
        {
            if (currentCol < 0 || currentCol >= Grid.Columns ||
                currentRow < 0 || currentRow >= Grid.Rows)
                return false;

            var tile = Grid.Tiles[currentCol, currentRow];
            int pathIndex = tile.GetPathIndexForSide(entrySide);
            if (pathIndex == -1) return false;

            if (visited.Contains((currentCol, currentRow, pathIndex)))
                return false;

            visited.Add((currentCol, currentRow, pathIndex));

            int exitSide = tile.GetExitSide(entrySide);
            if (exitSide == -1) return false;

            // Mark traversal with player color
            tile.IsTraversed = true;
            tile.TraversedPaths[pathIndex] = endpoint.Color;

            // Check if reached home
            if (currentRow == Grid.Rows - 1 && currentCol == endpoint.HomeColumn && exitSide == endpoint.HomeExitSide)
                return true;

            var (nextCol, nextRow) = GetNeighbor(currentCol, currentRow, exitSide);
            entrySide = (exitSide + 3) % 6;
            currentCol = nextCol;
            currentRow = nextRow;
        }

        return false;
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
