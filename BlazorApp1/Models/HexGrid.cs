namespace BlazorApp1.Models;

/// <summary>
/// Represents the full hex grid game state for Hex Me Home.
/// </summary>
public class HexGrid
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    public HexTile[,] Tiles { get; set; } = null!;

    /// <summary>
    /// Column where the player enters from the top edge.
    /// The player enters through the top side of tile [PlayerColumn, 0].
    /// </summary>
    public int PlayerColumn { get; set; }

    /// <summary>
    /// The side of the top-row tile that the player enters through.
    /// For flat-top hex with player on top: side 5 (top-left) or side 0 (top-right).
    /// We'll use side 5 for even columns, side 0 for odd columns — or just pick a top-facing side.
    /// Actually for simplicity: player always enters through side 5 (top-left) of the entry tile.
    /// </summary>
    public int PlayerEntrySide { get; set; }

    /// <summary>
    /// Column where the home is on the bottom edge.
    /// </summary>
    public int HomeColumn { get; set; }

    /// <summary>
    /// The side of the bottom-row tile that leads to home.
    /// </summary>
    public int HomeExitSide { get; set; }

    /// <summary>
    /// Whether the puzzle is solved.
    /// </summary>
    public bool IsSolved { get; set; }

    /// <summary>
    /// Whether the solution is currently being shown.
    /// </summary>
    public bool ShowingSolution { get; set; }
}

