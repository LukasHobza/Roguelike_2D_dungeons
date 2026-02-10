using Godot;
using System;

public partial class RunStats : Node
{
    public int EnemiesKilled { get; private set; }
    public int GoldAtLevelStart { get; private set; }
    public float LevelTime { get; private set; }

    private double levelStartTime;

    public void StartLevel(int currentGold)
    {
        EnemiesKilled = 0;
        GoldAtLevelStart = currentGold;
        levelStartTime = Time.GetTicksMsec() / 1000.0;

        GD.Print("GoldAtLevelStart: "+ GoldAtLevelStart);
    }

    public void EndLevel(int currentGold)
    {
        LevelTime = (float)(Time.GetTicksMsec() / 1000.0 - levelStartTime);
    }

    public void RegisterKill()
    {
        EnemiesKilled++;
    }

    public int GoldEarned(int currentGold)
    {
        return currentGold - GoldAtLevelStart;
    }
}
