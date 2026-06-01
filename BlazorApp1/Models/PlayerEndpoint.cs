namespace BlazorApp1.Models;

/// <summary>
/// Represents a player-home pair (start at top, home at bottom).
/// </summary>
public class PlayerEndpoint
{
    /// <summary>
    /// Column where the player enters from the top edge (row 0).
    /// </summary>
    public int PlayerColumn { get; set; }

    /// <summary>
    /// The side of the top-row tile that the player enters through.
    /// </summary>
    public int PlayerEntrySide { get; set; } = 5;

    /// <summary>
    /// Column where the home is on the bottom edge (last row).
    /// </summary>
    public int HomeColumn { get; set; }

    /// <summary>
    /// The side of the bottom-row tile that leads to home.
    /// </summary>
    public int HomeExitSide { get; set; } = 3;

    /// <summary>
    /// Player color for rendering.
    /// </summary>
    public string Color { get; set; } = "#e63946";

    /// <summary>
    /// Whether this specific player's path is solved.
    /// </summary>
    public bool IsSolved { get; set; }
}

