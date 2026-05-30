namespace BlazorApp1.Models;

/// <summary>
/// Represents a single hexagonal tile with 3 paths connecting pairs of its 6 sides.
/// Sides numbered 0-5 clockwise from top-right.
/// </summary>
public class HexTile
{
    /// <summary>
    /// The three paths on this tile. Each path is a pair of side indices (0-5).
    /// All 6 sides are used exactly once.
    /// </summary>
    public (int SideA, int SideB)[] Paths { get; set; } = new (int, int)[3];

    /// <summary>
    /// Current rotation index (0-5). Each increment = 60° clockwise.
    /// </summary>
    public int Rotation { get; set; }

    /// <summary>
    /// The rotation index that represents the solution state.
    /// </summary>
    public int SolutionRotation { get; set; }

    /// <summary>
    /// Grid position.
    /// </summary>
    public int Col { get; set; }
    public int Row { get; set; }

    /// <summary>
    /// Whether this tile is part of the solution path.
    /// </summary>
    public bool IsSolutionTile { get; set; }

    /// <summary>
    /// Whether this tile is currently highlighted (part of active traversal from player).
    /// </summary>
    public bool IsTraversed { get; set; }

    /// <summary>
    /// Which path index (0-2) is traversed, or -1 if none.
    /// </summary>
    public int TraversedPathIndex { get; set; } = -1;

    /// <summary>
    /// Get the effective side connections after rotation.
    /// A path connecting (a, b) at rotation R becomes ((a + R) % 6, (b + R) % 6).
    /// </summary>
    public (int SideA, int SideB) GetRotatedPath(int pathIndex)
    {
        var path = Paths[pathIndex];
        return ((path.SideA + Rotation) % 6, (path.SideB + Rotation) % 6);
    }

    /// <summary>
    /// Given an entry side on this tile (after rotation), find which path uses it
    /// and return the exit side. Returns -1 if no path uses that side.
    /// </summary>
    public int GetExitSide(int entrySide)
    {
        for (int i = 0; i < 3; i++)
        {
            var (a, b) = GetRotatedPath(i);
            if (a == entrySide) return b;
            if (b == entrySide) return a;
        }
        return -1;
    }

    /// <summary>
    /// Given an entry side, returns the path index that contains that side, or -1.
    /// </summary>
    public int GetPathIndexForSide(int entrySide)
    {
        for (int i = 0; i < 3; i++)
        {
            var (a, b) = GetRotatedPath(i);
            if (a == entrySide || b == entrySide) return i;
        }
        return -1;
    }

    public void Rotate()
    {
        Rotation = (Rotation + 1) % 6;
    }
}

