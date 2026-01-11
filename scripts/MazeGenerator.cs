using Godot;
using System;
using System.Collections.Generic;

public partial class MazeGenerator : Node2D
{
    [Export] public int Width = 31;
    [Export] public int Height = 21;
    [Export] public int TileSize = 32;
    [Export] public PackedScene DoorScene;

    private TileMapLayer floorLayer;
    private TileMapLayer wallLayer;
    private Node2D doorsParent;

    private int[,] maze;
    private Random rng = new();

    private readonly Vector2I[] directions =
    {
        new(0, -2),
        new(0, 2),
        new(-2, 0),
        new(2, 0)
    };

    public override void _Ready()
    {
        floorLayer = GetNode<TileMapLayer>("FloorLayer");
        wallLayer = GetNode<TileMapLayer>("WallLayer");
        doorsParent = GetNode<Node2D>("Doors");

        GenerateNewDungeon();
    }

    private void OnDoorEntered()
    {
        GD.Print("NEXT LEVEL");

        CallDeferred(nameof(GenerateNextLevel));
    }

    private void GenerateNextLevel()
    {
        GenerateNewDungeon();
    }

    private void GenerateNewDungeon()
    {
        floorLayer.Clear();
        wallLayer.Clear();

        foreach (Node c in doorsParent.GetChildren())
            c.QueueFree();

        GenerateMaze();
        DrawMaze();
        PlacePlayer();
        PlaceDoor();
    }

    // ---------- BLUDISTE ----------

    private void GenerateMaze()
    {
        maze = new int[Width, Height];

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                maze[x, y] = 1;

        Carve(new Vector2I(1, 1));
    }

    private void Carve(Vector2I pos)
    {
        maze[pos.X, pos.Y] = 0;

        var dirs = new List<Vector2I>(directions);
        Shuffle(dirs);

        foreach (var dir in dirs)
        {
            Vector2I next = pos + dir;
            if (IsInside(next) && maze[next.X, next.Y] == 1)
            {
                Vector2I between = pos + dir / 2;
                maze[between.X, between.Y] = 0;
                Carve(next);
            }
        }
    }

    private bool IsInside(Vector2I p)
        => p.X > 0 && p.Y > 0 && p.X < Width - 1 && p.Y < Height - 1;

    private void Shuffle(List<Vector2I> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ---------- VYKRESLENI ----------

    private void DrawMaze()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (maze[x, y] == 0)
                    floorLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(0,0));
                else
                    wallLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(1, 0));
            }
    }

    // ---------- HRAC SPAWN ----------

    private void PlacePlayer()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null) return;

        player.GlobalPosition = TileToWorld(new Vector2I(1, 1));
    }

    // ---------- DVERE SPAWN ----------

    private void PlaceDoor()
    {
        GD.Print("Spawn door");
        Vector2I cell = FindFarthestFloor();

        var door = DoorScene.Instantiate<Door>();
        door.GlobalPosition = TileToWorld(cell);

        door.DoorEntered += OnDoorEntered;

        doorsParent.AddChild(door);
    }


    private Vector2I FindFarthestFloor()
    {
        Vector2I start = new(1, 1);
        Vector2I best = start;
        int maxDist = 0;

        for (int x = 1; x < Width - 1; x++)
            for (int y = 1; y < Height - 1; y++)
            {
                if (maze[x, y] == 0)
                {
                    int dist = Mathf.Abs(x - start.X) + Mathf.Abs(y - start.Y);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        best = new Vector2I(x, y);
                    }
                }
            }
        return best;
    }

    //----

    private Vector2 TileToWorld(Vector2I t)
        => new(
            t.X * TileSize + TileSize / 2,
            t.Y * TileSize + TileSize / 2
        );
}
