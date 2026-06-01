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
    /// All player-home endpoint pairs. Each must be connected for a win.
    /// </summary>
    public List<PlayerEndpoint> Endpoints { get; set; } = new();

    // Legacy accessors for backward compatibility
    public int PlayerColumn => Endpoints.Count > 0 ? Endpoints[0].PlayerColumn : 0;
    public int PlayerEntrySide => Endpoints.Count > 0 ? Endpoints[0].PlayerEntrySide : 5;
    public int HomeColumn => Endpoints.Count > 0 ? Endpoints[0].HomeColumn : 0;
    public int HomeExitSide => Endpoints.Count > 0 ? Endpoints[0].HomeExitSide : 3;

    /// <summary>
    /// Whether ALL players are connected to their homes.
    /// </summary>
    public bool IsSolved { get; set; }

    /// <summary>
    /// Whether the solution is currently being shown.
    /// </summary>
    public bool ShowingSolution { get; set; }
}
