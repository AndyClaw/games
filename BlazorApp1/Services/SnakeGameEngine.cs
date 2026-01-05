using BlazorApp1.Models;

namespace BlazorApp1.Services;

/// <summary>
/// Encapsulates all snake game logic including movement, gravity, collision detection
/// </summary>
public class SnakeGameEngine
{
    private const int GridSize = 20;
    private const int GravityAnimationDelayMs = 200;

    // Game state
    public List<Position> Snake { get; private set; } = new();
    public List<Position> Apples { get; private set; } = new();
    public Position PortalPosition { get; private set; } = new(0, 0);
    public List<Position> GroundBlocks { get; private set; } = new();
    public List<Position> PushableBlocks { get; private set; } = new();
    public List<Position> Bombs { get; private set; } = new();
    
    public bool LevelWon { get; private set; }
    public bool GameOver { get; private set; }
    public string GameOverReason { get; private set; } = "";
    public bool IsAnimating { get; private set; }

    // Events for UI updates
    public event Action? OnStateChanged;

    public void InitializeGame(List<Position> snake, List<Position> apples, Position portal, 
        List<Position> groundBlocks, List<Position> pushableBlocks, List<Position> bombs)
    {
        Snake = new List<Position>(snake);
        Apples = new List<Position>(apples);
        PortalPosition = portal;
        GroundBlocks = new List<Position>(groundBlocks);
        PushableBlocks = new List<Position>(pushableBlocks);
        Bombs = new List<Position>(bombs);
        LevelWon = false;
        GameOver = false;
        GameOverReason = "";
        IsAnimating = false;
    }

    public async Task MoveSnakeWithAnimatedGravity(Direction direction)
    {
        if (SnakeMovedNoGravityNeeded(direction)) return;
        await ApplyGravityAnimatedAsync();
    }

    private bool SnakeMovedNoGravityNeeded(Direction direction)
    {
        if (Snake.Count == 0 || LevelWon || GameOver || IsAnimating)
            return true;

        var head = Snake[0];
        Position newHead = direction switch
        {
            Direction.Up => head.Up(),
            Direction.Down => head.Down(),
            Direction.Left => head.Left(),
            Direction.Right => head.Right(),
            _ => head
        };

        // Check if portal was reached
        if (newHead.Equals(PortalPosition))
        {
            LevelWon = true;
            NotifyStateChanged();
            return true;
        }

        // Check if out of bounds
        if (newHead.Row < 0 || newHead.Row >= GridSize || newHead.Col < 0 || newHead.Col >= GridSize)
            return true;

        // Check if hitting itself
        if (Snake.Contains(newHead))
            return true;

        // Check if hitting ground block
        if (GroundBlocks.Contains(newHead))
            return true;

        // Check if hitting a pushable block
        if (PushableBlocks.Contains(newHead))
        {
            Position blockDestination = direction switch
            {
                Direction.Up => newHead.Up(),
                Direction.Down => newHead.Down(),
                Direction.Left => newHead.Left(),
                Direction.Right => newHead.Right(),
                _ => newHead
            };

            // Check if block can be pushed
            if (blockDestination.Row < 0 || blockDestination.Row >= GridSize ||
                blockDestination.Col < 0 || blockDestination.Col >= GridSize)
                return true;

            if (GroundBlocks.Contains(blockDestination) ||
                PushableBlocks.Contains(blockDestination) ||
                Snake.Any(s => s.Equals(blockDestination)) ||
                Apples.Contains(blockDestination))
                return true;

            // Push the block
            PushableBlocks.Remove(newHead);
            PushableBlocks.Add(blockDestination);
        }

        // Check if hitting a bomb
        if (Bombs.Contains(newHead) && !PushableBlocks.Contains(newHead))
        {
            GameOver = true;
            GameOverReason = "You hit a bomb!";
            NotifyStateChanged();
            return true;
        }

        // Insert new head
        Snake.Insert(0, newHead);

        // Check if apple was collected
        bool ateApple = false;
        if (Apples.Contains(newHead))
        {
            Apples.Remove(newHead);
            ateApple = true;
        }

        // Remove tail (unless we ate an apple)
        if (!ateApple)
        {
            Snake.RemoveAt(Snake.Count - 1);
        }

        NotifyStateChanged();
        // no gravity needed if tail is still where it was
        if (ateApple) return true;
        return false;
    }


    private async Task ApplyGravityAnimatedAsync()
    {
        IsAnimating = true;
        NotifyStateChanged();

        try
        {
            await ApplyGravityToPushableBlocksAnimatedAsync();
            await ApplyGravityToSnakeAnimatedAsync();
        }
        finally
        {
            IsAnimating = false;
            NotifyStateChanged();
        }
    }

    private async Task ApplyGravityToPushableBlocksAnimatedAsync()
    {
        bool blocksStillFalling = true;

        while (blocksStillFalling)
        {
            blocksStillFalling = false;
            List<Position> newPushableBlocks = new List<Position>(PushableBlocks);

            foreach (var block in PushableBlocks.OrderByDescending(b => b.Row))
            {
                var belowPos = block.Down();

                if (belowPos.Row < GridSize &&
                    !GroundBlocks.Contains(belowPos) &&
                    !newPushableBlocks.Contains(belowPos) &&
                    !Snake.Any(s => s.Equals(belowPos)) &&
                    !Apples.Contains(belowPos) &&
                    !belowPos.Equals(PortalPosition))
                {
                    newPushableBlocks.Remove(block);
                    newPushableBlocks.Add(belowPos);
                    blocksStillFalling = true;
                }
            }

            PushableBlocks = newPushableBlocks;

            if (blocksStillFalling)
            {
                NotifyStateChanged();
                await Task.Delay(GravityAnimationDelayMs);
            }
        }
    }

    private async Task ApplyGravityToSnakeAnimatedAsync()
    {
        while (!IsSnakeSupported())
        {
            int maxRow = Snake.Max(s => s.Row);
            if (maxRow >= GridSize - 1)
            {
                GameOver = true;
                GameOverReason = "You fell off the bottom!";
                NotifyStateChanged();
                return;
            }

            for (int i = 0; i < Snake.Count; i++)
            {
                Snake[i] = Snake[i].Down();
            }

            foreach (var segment in Snake)
            {
                if (Bombs.Contains(segment) && !PushableBlocks.Contains(segment))
                {
                    GameOver = true;
                    GameOverReason = "You hit a bomb while falling!";
                    NotifyStateChanged();
                    return;
                }
            }

            NotifyStateChanged();
            await Task.Delay(GravityAnimationDelayMs);
        }
    }


    private bool IsSnakeSupported()
    {
        return Snake.Any(segment => PositionProvidesSupport(segment.Down()));
    }

    private bool PositionProvidesSupport(Position belowPos)
    {
        return GroundBlocks.Contains(belowPos) ||
               PushableBlocks.Contains(belowPos) ||
               Apples.Contains(belowPos) ||
               belowPos.Equals(PortalPosition);
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

