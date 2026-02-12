using Godot;
using System;
using System.Collections.Generic;

public partial class MazeGenerator : Node2D
{
    [Export] public int Width = 31;
    [Export] public int Height = 21;
    [Export] public int TileSize = 32;
    [Export] public PackedScene DoorScene;
    [Export] public PackedScene ZombieScene;
    [Export] public int BaseEnemyCount = 3;

    private TileMapLayer floorLayer;
    private TileMapLayer wallLayer;
    private Node2D doorsParent;

    private int[,] maze;
    private Random rng = new();

    private int dungeonLevel = 1;
    private int aliveEnemies = 0;
    private bool doorSpawned = false;

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

        CallDeferred(nameof(GenerateNewDungeon));
    }

    private void OnDoorEntered()
    {
        GD.Print("NEXT LEVEL");

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        var stats = GetNode<RunStats>("/root/RunStats");

        stats.EndLevel(player.Gold);

        ShowSummary();



        //CallDeferred(nameof(GenerateNextLevel));
    }

    private void ShowSummary()
    {
        var summaryScene = GD.Load<PackedScene>("res://scenes/EndLevelSummary.tscn");
        var summaryNode = summaryScene.Instantiate();
        var summary = summaryNode as EndLevelSummary;
        if (summary == null)
        {
            GD.PrintErr("EndLevelSummary script není přiřazen root node!");
        }

        summary.NextLevelRequested += () =>
        {
            dungeonLevel++;
            GenerateNewDungeon();
        };

        GetTree().Root.AddChild(summary);
    }

    private void GenerateNextLevel()
    {
        dungeonLevel++;
        GenerateNewDungeon();
    }


    private void GenerateNewDungeon()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        GetNode<RunStats>("/root/RunStats").StartLevel(player.Gold);

        doorSpawned = false;
        floorLayer.Clear();
        wallLayer.Clear();

        foreach (Node c in doorsParent.GetChildren())
            c.QueueFree();

        foreach (Node e in GetTree().GetNodesInGroup("enemy"))
            e.QueueFree();

        foreach (Node item in GetTree().GetNodesInGroup("ground_item"))
            item.QueueFree();

        PlacePlayer();

        if (IsBossLevel())
        {
            GenerateBossRoom();
        }
        else
        {
            GenerateMaze();
            DrawMaze();
            PlaceEnemies();
        }
        
        
    }

    private bool IsBossLevel()
    {
        return dungeonLevel % 3 == 0;
    }

    private void GenerateBossRoom()
    {
        maze = new int[Width, Height];
        aliveEnemies = 1;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // okraje = zdi
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                    maze[x, y] = 1;
                else
                    maze[x, y] = 0;
            }
        }

        DrawMaze();
        PlaceBoss();
    }

    private void PlaceBoss()
    {
        Vector2I center = new(Width / 2, Height / 2);

        var boss = ZombieScene.Instantiate<Enemy>();
        boss.EnemyDied += OnEnemyDied;
        // hodne silny
        boss.MaxHP = 200 + dungeonLevel * 50;
        boss.Damage = 30 + dungeonLevel * 5;
        boss.Speed = 50;

        boss.CurrentHP = boss.MaxHP;

        boss.GlobalPosition = TileToWorld(center);
        AddChild(boss);

        GD.Print("BOSS SPANWED");
    }

    private void PlaceEnemies()
    {
        int enemyCount = BaseEnemyCount + dungeonLevel;
        aliveEnemies = enemyCount;
        GD.Print("enemy count: " + enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = ZombieScene.Instantiate<Enemy>();

            enemy.EnemyDied += OnEnemyDied;

            ScaleEnemyStats(enemy);
            enemy.GlobalPosition = TileToWorld(
                GetRandomFloorCellFarFromPlayer(5)
            );

            AddChild(enemy);
        }
    }

    private void OnEnemyDied(Enemy enemy)
    {
        aliveEnemies--;

        GD.Print($"Enemy umrel, zbyva: {aliveEnemies}");

        if (aliveEnemies <= 0 && !doorSpawned)
        {
            doorSpawned = true;
            CallDeferred(nameof(PlaceDoor));
        }
    }

    private Vector2I GetRandomFloorCellFarFromPlayer(int minDistance)
    {
        Vector2I playerCell = new(1, 1);

        while (true)
        {
            int x = rng.Next(1, Width - 1);
            int y = rng.Next(1, Height - 1);

            if (maze[x, y] != 0)
                continue;

            int dist = Mathf.Abs(x - playerCell.X) + Mathf.Abs(y - playerCell.Y);
            if (dist < minDistance)
                continue;

            return new Vector2I(x, y);
        }
    }

    private void ScaleEnemyStats(Enemy enemy)
    {
        enemy.MaxHP += dungeonLevel * 5;
        enemy.Damage += dungeonLevel * 2;
        enemy.Speed += dungeonLevel * 2;

        enemy.CurrentHP = enemy.MaxHP;
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

        var door = DoorScene.Instantiate<Door>();
        door.GlobalPosition = TileToWorld(new Vector2I(Width-2, Height-2));

        door.DoorEntered += OnDoorEntered;

        doorsParent.AddChild(door);
    }

    //----

    private Vector2 TileToWorld(Vector2I t)
        => new(
            t.X * TileSize + TileSize / 2,
            t.Y * TileSize + TileSize / 2
        );
}
