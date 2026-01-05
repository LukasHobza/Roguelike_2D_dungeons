using Godot;
using System;

public partial class GameDatabase : Node
{
    private string savePath;
    private int _gold;

    public override void _Ready()
    {
        savePath = "user://save.cfg";
        Load();
    }

    private void Load()
    {
        var cfg = new ConfigFile();
        var err = cfg.Load(savePath);
        if (err == Error.Ok)
        {
            _gold = (int)cfg.GetValue("player", "gold", 0);
        }
        else
        {
            _gold = 0;
        }
    }

    private void Save()
    {
        var cfg = new ConfigFile();
        cfg.SetValue("player", "gold", _gold);
        cfg.Save(savePath);
    }

    //veřejné metody pro Player
    public int LoadGold()
    {
        return _gold;
    }

    public void SaveGold(int gold)
    {
        _gold = gold;
        Save();
    }
}