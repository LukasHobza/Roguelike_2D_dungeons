using Godot;
using System;

public partial class RunStats : Node
{
    // pocet zabitych nepratel
    public int EnemiesKilled { get; private set; }
    public int GoldAtLevelStart { get; private set; }
    public float LevelTime { get; private set; }

    // vnitrni promenna pro cas spusteni
    private double levelStartTime;

    public void StartLevel(int currentGold)
    {
        EnemiesKilled = 0;
        GoldAtLevelStart = currentGold;
        // ziskani aktualniho casu v sekundach
        levelStartTime = Time.GetTicksMsec() / 1000.0;

        GD.Print("GoldAtLevelStart: "+ GoldAtLevelStart);
    }

    public void EndLevel(int currentGold)
    {
        // vypocet rozdilu casu od startu
        LevelTime = (float)(Time.GetTicksMsec() / 1000.0 - levelStartTime);
    }

    public void RegisterKill()
    {
        EnemiesKilled++;
    }

    public int GoldEarned(int currentGold)
    {
        // rozdil mezi aktualnim a pocatecnim zlatem
        return currentGold - GoldAtLevelStart;
    }
}
