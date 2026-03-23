using Godot;
using System;
using System.Collections.Generic;

public partial class MazeGenerator : Node2D
{
    // nastaveni rozmeru a velikosti dlazdice
    [Export] public int Width = 31;// sirka bludiste (musí být liché číslo pro správné zdi)
    [Export] public int Height = 21;
    [Export] public int TileSize = 32;
    // sceny pro objekty ktere se budou vytvaret
    [Export] public PackedScene DoorScene;
    [Export] public PackedScene ZombieScene;
    [Export] public PackedScene BossScene;
    // zakladni pocet nepratel pro prvni level
    [Export] public int BaseEnemyCount = 3;

    // odkazy na vrstvy mapy a rodice dveri
    private TileMapLayer floorLayer;
    private TileMapLayer wallLayer;
    private Node2D doorsParent;

    // dvourozmerne pole reprezentujici mrizku bludiste (0 = podlaha, 1 = zed)
    private int[,] maze;
    private Random rng = new();

    // stavove promenne pro hru
    private int dungeonLevel = 1;
    private int aliveEnemies = 0;
    private bool doorSpawned = false;

    // smery pohybu pri generovani (skace o 2 policka kvuli zachovani zdi mezi chodbami)
    private readonly Vector2I[] directions =
    {
        new(0, -2),// nahoru
        new(0, 2),
        new(-2, 0),
        new(2, 0)
    };

    public override void _Ready()
    {
        // nacteni uzlu z editoru
        floorLayer = GetNode<TileMapLayer>("FloorLayer");
        wallLayer = GetNode<TileMapLayer>("WallLayer");
        doorsParent = GetNode<Node2D>("Doors");

        // odlozeni generovani na dalsi snimek aby bylo vse pripraveno
        CallDeferred(nameof(GenerateNewDungeon));
    }

    // vola se kdyz hrac vejde do dveri na konci levelu
    private void OnDoorEntered()
    {
        GD.Print("NEXT LEVEL");

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        var stats = GetNode<RunStats>("/root/RunStats");

        // ukonceni mereni casu a ulozeni statistik
        stats.EndLevel(player.Gold);

        // zobrazeni souhrnu statistik
        ShowSummary();
    }

    // vytvoreni a zobrazeni okna s vysledky levelu
    private void ShowSummary()
    {
        // nacte scenu z disku jako "vzor" (packedscene)
        var summaryScene = GD.Load<PackedScene>("res://scenes/EndLevelSummary.tscn");
        // podle vzoru vytvori konkretni objekt (uzel) v pameti
        var summaryNode = summaryScene.Instantiate();
        // rekne c#, ze tento uzel ma skript typu endlevelsummary, aby slo pouzivat jeho funkce
        var summary = summaryNode as EndLevelSummary;
        if (summary == null)
        {
            GD.PrintErr("EndLevelSummary script není přiřazen root node!");
        }

        // reakce na tlacitko pro dalsi level v souhrnu
        summary.NextLevelRequested += () =>
        {
            dungeonLevel++;
            // pokud prichazi boss level tak zvetsi mapu
            if (IsBossLevel())
            {
                Width += 2;
                Height += 2;
            }
            GenerateNewDungeon();
        };

        GetTree().Root.AddChild(summary);
    }

    // hlavni ridici funkce pro novy dungeon
    private void GenerateNewDungeon()
    {
        // resetujeme statistiky pro nove patro
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        GetNode<RunStats>("/root/RunStats").StartLevel(player.Gold);

        doorSpawned = false;
        floorLayer.Clear();// vymazeme starou podlahu
        wallLayer.Clear();// vymazeme stare zdi

        // vycisteni starych dveri, enemaku a itemu na zemi
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
        Vector2I center = new(Width / 2, Height / 2);// stred areny

        var boss = BossScene.Instantiate<Enemy>();
        boss.EnemyDied += OnEnemyDied;// pripojime signal smrti
        // hodne silny
        boss.MaxHP += dungeonLevel * 20;
        boss.Damage += dungeonLevel * 8;
        boss.CurrentHP = boss.MaxHP;

        boss.GlobalPosition = TileToWorld(center);// prepocet souradnic
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

            ScaleEnemyStats(enemy); // vylepsime zombika

            // najde nahodne misto, ale ne hned u hrace (min 5 dlazdic daleko)
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

        // kdyz jsou vsichni mrtvi tak se spawnou dvere
        if (aliveEnemies <= 0 && !doorSpawned)
        {
            doorSpawned = true;
            CallDeferred(nameof(PlaceDoor)); // spawn dvere bezpecne az po fyzice
        }
    }

    //  zpusob jak najit volne misto pro spawn
    private Vector2I GetRandomFloorCellFarFromPlayer(int minDistance)
    {
        Vector2I playerCell = new(1, 1);

        while (true)
        {
            int x = rng.Next(1, Width - 1);
            int y = rng.Next(1, Height - 1);

            // policko musi byt podlaha (0)
            if (maze[x, y] != 0)
                continue;

            // vypocet vzdalenosti
            int dist = Mathf.Abs(x - playerCell.X) + Mathf.Abs(y - playerCell.Y);
            if (dist < minDistance)
                continue;// moc blizko zkusi znova

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

    // ---------- LOGIKA GENERACE BLUDISTE ----------
    // ---------- LOGIKA GENERACE BLUDISTE ----------
    // ---------- LOGIKA GENERACE BLUDISTE ----------

    private void GenerateMaze()
    {
        maze = new int[Width, Height];

        // 1. naplni cele pole zdmi (1)
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                maze[x, y] = 1;

        // 2. spusti rekurzivni vyrezavani chodeb od startu (1,1)
        Carve(new Vector2I(1, 1));
    }

    // rekurzivni funkce "recursive backtracker"
    private void Carve(Vector2I pos)
    {
        maze[pos.X, pos.Y] = 0; // aktualni policko zmenime na podlahu

        // vytvore seznam smeru a zamicha ho (nahodnost bludiste)
        var dirs = new List<Vector2I>(directions);
        Shuffle(dirs);

        foreach (var dir in dirs)
        {
            Vector2I next = pos + dir;// cilove policko (o 2 dal)

            // zkontrolujeme, jestli jsme v poli a jestli je tam jeste zed
            if (IsInside(next) && maze[next.X, next.Y] == 1)
            {
                // pokud ano, vybourame i tu zed MEZI aktualnim a pristim polickem
                Vector2I between = pos + dir / 2;
                maze[between.X, between.Y] = 0;

                // skocime do noveho policka a pokracujeme odtud (rekurze)
                Carve(next);
            }
        }
    }

    // kontrola aby generator nevyjel z mapy (nechavame 1 radek okraj)
    private bool IsInside(Vector2I p)
        => p.X > 0 && p.Y > 0 && p.X < Width - 1 && p.Y < Height - 1;

    // klasicky shuffle pro nahodne zamichani listu
    private void Shuffle(List<Vector2I> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            // vybereme nahodny index 'j' v rozsahu od 0 do 'i' (vcetne i)
            // tim zajistime, ze vybirame z tech prvků, ktere jsme jeste neprohodili
            int j = rng.Next(i + 1);

            // prohodime prvek na pozici 'i' s prvkem na nahodne pozici 'j'
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ---------- VYKRESLENI ----------

    private void DrawMaze()
    {
        // projdeme celou matrici 'maze'
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (maze[x, y] == 0)
                    // nastavime dlazdici podlahy (zdroj 2, atlas souradnice 0,0)
                    floorLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(0,0));
                else
                    // nastavime dlazdici zdi (zdroj 2, atlas souradnice 1,0)
                    wallLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(1, 0));
            }
    }

    // ---------- HRAC SPAWN ----------

    private void PlacePlayer()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null) return;

        // hrac startuje vzdy vlevo nahore na souradnicich 1,1
        player.GlobalPosition = TileToWorld(new Vector2I(1, 1));
    }

    // ---------- DVERE SPAWN ----------

    private void PlaceDoor()
    {


        GD.Print("Spawn door");

        var door = DoorScene.Instantiate<Door>();
        // dvere se vzdy objevi v pravém dolnim rohu bludiste
        door.GlobalPosition = TileToWorld(new Vector2I(Width-2, Height-2));

        door.DoorEntered += OnDoorEntered;

        doorsParent.AddChild(door);
    }

    // pomocna funkce pro vypocet: index dlazdice * velikost dlazdice + pulka (stred)
    private Vector2 TileToWorld(Vector2I t)
        => new(
            t.X * TileSize + TileSize / 2,
            t.Y * TileSize + TileSize / 2
        );
}
