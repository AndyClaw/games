namespace BlazorApp1.Models;

public record LevelConfig(
    string Description,
    List<Position> SnakeSegments,
    List<Position> Apples,
    Position Portal,
    List<Position> GroundBlocks,
    List<Position> PushableBlocks,
    List<Position> Bombs
)
{
    public static LevelConfig Parse(string compact)
    {
        var description = "";
        var snakeSegments = new List<Position>();
        var apples = new List<Position>();
        Position? portal = null;
        var groundBlocks = new List<Position>();
        var pushableBlocks = new List<Position>();
        var bombs = new List<Position>();

        var sections = compact.Split('&');
        foreach (var section in sections)
        {
            if (string.IsNullOrWhiteSpace(section)) continue;
            
            var equalIndex = section.IndexOf('=');
            if (equalIndex < 0) continue;

            var sectionType = section[0];
            var sectionData = section.Substring(equalIndex + 1);

            switch (sectionType)
            {
                case 'D':
                    description = System.Net.WebUtility.UrlDecode(sectionData);
                    break;
                case 'E':
                    portal = ParsePosition(sectionData);
                    break;
                case 'S':
                    snakeSegments = ParsePositions(sectionData);
                    break;
                case 'A':
                    apples = ParsePositions(sectionData);
                    break;
                case 'G':
                    groundBlocks = ParsePositions(sectionData);
                    break;
                case 'P':
                    pushableBlocks = ParsePositions(sectionData);
                    break;
                case 'B':
                    bombs = ParsePositions(sectionData);
                    break;
            }
        }

        return new LevelConfig(
            description,
            snakeSegments,
            apples,
            portal ?? new Position(0, 0),
            groundBlocks,
            pushableBlocks,
            bombs
        );
    }

    private static Position ParsePosition(string pos)
    {
        var parts = pos.Split(',');
        return new Position(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static List<Position> ParsePositions(string positions)
    {
        if (string.IsNullOrWhiteSpace(positions))
            return new List<Position>();

        return positions.Split(';')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => ParsePosition(p))
            .ToList();
    }

    public string ToCompactString()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(Description))
            parts.Add($"D={System.Net.WebUtility.UrlEncode(Description)}");
        
        parts.Add($"E={Portal.Row},{Portal.Col}");
        
        if (SnakeSegments.Any())
            parts.Add($"S={string.Join(";", SnakeSegments.Select(p => $"{p.Row},{p.Col}"))}");
        
        if (Apples.Any())
            parts.Add($"A={string.Join(";", Apples.Select(p => $"{p.Row},{p.Col}"))}");
        
        if (GroundBlocks.Any())
            parts.Add($"G={string.Join(";", GroundBlocks.Select(p => $"{p.Row},{p.Col}"))}");
        
        if (PushableBlocks.Any())
            parts.Add($"P={string.Join(";", PushableBlocks.Select(p => $"{p.Row},{p.Col}"))}");
        
        if (Bombs.Any())
            parts.Add($"B={string.Join(";", Bombs.Select(p => $"{p.Row},{p.Col}"))}");
        
        return string.Join("&", parts);
    }
}

