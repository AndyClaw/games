namespace BlazorApp1.Models;

public record Position(int Row, int Col)
{
    public Position Up() => new(Row - 1, Col);
    public Position Down() => new(Row + 1, Col);
    public Position Left() => new(Row, Col - 1);
    public Position Right() => new(Row, Col + 1);
}

